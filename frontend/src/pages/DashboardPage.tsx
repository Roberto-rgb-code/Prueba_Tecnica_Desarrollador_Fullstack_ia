import { useSearchParams } from 'react-router-dom';
import UsersTab from '../components/UsersTab';
import RolesTab from '../components/RolesTab';
import AuditTab from '../components/AuditTab';
import AgentTab from '../components/AgentTab';

export default function DashboardPage() {
  const [params] = useSearchParams();
  const tab = params.get('tab') ?? 'users';

  switch (tab) {
    case 'roles': return <RolesTab />;
    case 'audit': return <AuditTab />;
    case 'agent': return <AgentTab />;
    default: return <UsersTab />;
  }
}
