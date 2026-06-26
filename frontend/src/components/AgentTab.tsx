import { FormEvent, useState } from 'react';
import { agentApi } from '../api/client';

export default function AgentTab() {
  const [question, setQuestion] = useState('');
  const [answer, setAnswer] = useState<string | null>(null);
  const [metrics, setMetrics] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const { data } = await agentApi.query(question);
      setAnswer(data.answer);
      setMetrics(`Latencia: ${data.metrics.latencyMs}ms | Tokens: ${data.metrics.inputTokens}+${data.metrics.outputTokens} | Costo: $${data.metrics.estimatedCostUsd.toFixed(6)}`);
    } catch {
      setError('Error al consultar el agente');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>Agente IA</h2>
      <p>Pregunta sobre usuarios, roles, autenticación o auditoría del sistema Toka.</p>
      <form className="form-col" onSubmit={handleSubmit}>
        <textarea rows={3} placeholder="¿Cómo funciona la autenticación en Toka?" value={question} onChange={(e) => setQuestion(e.target.value)} required />
        <button type="submit" disabled={loading}>{loading ? 'Consultando...' : 'Preguntar'}</button>
      </form>
      {error && <div className="error">{error}</div>}
      {answer && <div className="answer-card"><h3>Respuesta</h3><p>{answer}</p>{metrics && <small>{metrics}</small>}</div>}
    </div>
  );
}
