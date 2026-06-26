import { FormEvent, useEffect, useRef, useState } from 'react';
import { Bot, Loader2, Send, Sparkles, Zap } from 'lucide-react';
import { toast } from 'sonner';
import { agentApi } from '../api/client';
import { cn } from '../lib/utils';

interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  sources?: string[];
  metrics?: string;
  isDemo?: boolean;
  llmProvider?: string;
}

const SUGGESTIONS = [
  '¿Cómo funciona la autenticación?',
  '¿Qué roles existen en el sistema?',
  '¿Cómo se registran los eventos de auditoría?',
  '¿Cómo creo un usuario?',
];

function renderMarkdownLite(text: string) {
  return text
    .split('\n')
    .map((line, i) => {
      let html = line
        .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
        .replace(/`(.+?)`/g, '<code class="rounded bg-slate-100 px-1 py-0.5 text-xs text-toka-700">$1</code>');
      if (line.startsWith('### '))
        html = `<h4 class="mt-3 font-semibold text-slate-900">${line.slice(4)}</h4>`;
      else if (line === '---') html = '<hr class="my-3 border-slate-200" />';
      return <p key={i} className="leading-relaxed" dangerouslySetInnerHTML={{ __html: html || '&nbsp;' }} />;
    });
}

export default function AgentTab() {
  const [question, setQuestion] = useState('');
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      id: 'welcome',
      role: 'assistant',
      content:
        '¡Hola! Soy **Toka Assistant**. Pregúntame sobre usuarios, roles, autenticación JWT o auditoría del sistema.',
    },
  ]);
  const [loading, setLoading] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, loading]);

  const ask = async (text: string) => {
    const q = text.trim();
    if (!q || loading) return;

    const userMsg: ChatMessage = { id: crypto.randomUUID(), role: 'user', content: q };
    setMessages((prev) => [...prev, userMsg]);
    setQuestion('');
    setLoading(true);

    try {
      const { data } = await agentApi.query(q);
      const metrics = `⚡ ${data.metrics.latencyMs}ms · 🎯 ${data.metrics.inputTokens}+${data.metrics.outputTokens} tokens · 💰 $${data.metrics.estimatedCostUsd.toFixed(6)}`;

      setMessages((prev) => [
        ...prev,
        {
          id: crypto.randomUUID(),
          role: 'assistant',
          content: data.answer,
          sources: data.sources,
          metrics,
          isDemo: data.isDemoMode,
          llmProvider: data.llmProvider,
        },
      ]);
    } catch {
      toast.error('No se pudo consultar al agente. Verifica que el servicio esté activo.');
      setMessages((prev) => [
        ...prev,
        {
          id: crypto.randomUUID(),
          role: 'assistant',
          content: 'Lo siento, hubo un error al procesar tu consulta. Intenta de nuevo en unos segundos.',
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    ask(question);
  };

  return (
    <div className="flex h-[calc(100vh-12rem)] min-h-[520px] flex-col">
      <div className="mb-4 flex items-start justify-between gap-4">
        <div>
          <h2 className="flex items-center gap-2 text-2xl font-bold text-slate-900">
            <Bot className="h-7 w-7 text-toka-600" />
            Agente IA
          </h2>
          <p className="mt-1 text-sm text-slate-500">
            RAG sobre usuarios, roles, autenticación y auditoría
          </p>
        </div>
        <div className="hidden rounded-xl border border-emerald-200 bg-emerald-50 px-3 py-2 text-xs text-emerald-800 sm:block">
          <Sparkles className="mb-1 inline h-3.5 w-3.5" /> Powered by Ollama local (llama3.2)
        </div>
      </div>

      {/* Chat */}
      <div className="card flex flex-1 flex-col overflow-hidden !p-0">
        <div className="flex-1 space-y-4 overflow-y-auto p-4 sm:p-6">
          {messages.map((msg) => (
            <div
              key={msg.id}
              className={cn('flex', msg.role === 'user' ? 'justify-end' : 'justify-start')}
            >
              <div
                className={cn(
                  'max-w-[85%] rounded-2xl px-4 py-3 text-sm shadow-sm sm:max-w-[75%]',
                  msg.role === 'user'
                    ? 'rounded-br-md bg-toka-600 text-white'
                    : 'rounded-bl-md border border-slate-100 bg-slate-50 text-slate-800'
                )}
              >
                {msg.role === 'assistant' && (
                  <div className="mb-2 flex flex-wrap items-center gap-2">
                    <span className="inline-flex items-center gap-1 rounded-full bg-toka-100 px-2 py-0.5 text-xs font-medium text-toka-700">
                      <Bot className="h-3 w-3" /> Toka AI
                    </span>
                    {msg.isDemo && (
                      <span className="rounded-full bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-700">
                        Demo local
                      </span>
                    )}
                    {msg.llmProvider === 'Ollama' && (
                      <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-medium text-emerald-700">
                        Ollama · {msg.llmProvider}
                      </span>
                    )}
                    {msg.llmProvider === 'OpenAi' && (
                      <span className="rounded-full bg-violet-100 px-2 py-0.5 text-xs font-medium text-violet-700">
                        OpenAI
                      </span>
                    )}
                  </div>
                )}
                <div className={msg.role === 'user' ? '' : 'space-y-1'}>
                  {msg.role === 'user' ? msg.content : renderMarkdownLite(msg.content)}
                </div>
                {msg.sources && msg.sources.length > 0 && (
                  <div className="mt-3 flex flex-wrap gap-1.5 border-t border-slate-200/80 pt-2">
                    {msg.sources.map((s) => (
                      <span key={s} className="rounded-lg bg-white px-2 py-0.5 text-xs text-slate-600 ring-1 ring-slate-200">
                        📚 {s}
                      </span>
                    ))}
                  </div>
                )}
                {msg.metrics && (
                  <p className="mt-2 border-t border-slate-200/80 pt-2 text-xs text-slate-500">{msg.metrics}</p>
                )}
              </div>
            </div>
          ))}

          {loading && (
            <div className="flex justify-start">
              <div className="flex items-center gap-2 rounded-2xl rounded-bl-md border border-slate-100 bg-slate-50 px-4 py-3 text-sm text-slate-500">
                <Loader2 className="h-4 w-4 animate-spin text-toka-600" />
                Consultando base de conocimiento...
              </div>
            </div>
          )}
          <div ref={bottomRef} />
        </div>

        {/* Sugerencias */}
        {messages.length <= 2 && (
          <div className="flex flex-wrap gap-2 border-t border-slate-100 px-4 py-3">
            {SUGGESTIONS.map((s) => (
              <button
                key={s}
                type="button"
                onClick={() => ask(s)}
                className="rounded-full border border-slate-200 bg-white px-3 py-1.5 text-xs text-slate-600 transition hover:border-toka-300 hover:bg-toka-50 hover:text-toka-700"
              >
                {s}
              </button>
            ))}
          </div>
        )}

        {/* Input */}
        <form onSubmit={handleSubmit} className="border-t border-slate-100 p-4">
          <div className="flex gap-2">
            <textarea
              rows={2}
              className="input-field min-h-[48px] resize-none"
              placeholder="Escribe tu pregunta..."
              value={question}
              onChange={(e) => setQuestion(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                  e.preventDefault();
                  handleSubmit(e);
                }
              }}
            />
            <button type="submit" className="btn-primary shrink-0 self-end !px-4" disabled={loading || !question.trim()}>
              {loading ? <Loader2 className="h-5 w-5 animate-spin" /> : <Send className="h-5 w-5" />}
            </button>
          </div>
          <p className="mt-2 flex items-center gap-1 text-xs text-slate-400">
            <Zap className="h-3 w-3" /> Enter para enviar · Shift+Enter nueva línea
          </p>
        </form>
      </div>
    </div>
  );
}
