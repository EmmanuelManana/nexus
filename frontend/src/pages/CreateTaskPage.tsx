import { useNavigate } from 'react-router-dom';
import { TaskForm } from '../components/TaskForm';
import { useCreateTask } from '../hooks/useCreateTask';
import { useTasksContext } from '../context/TasksContext';

export function CreateTaskPage() {
  const navigate = useNavigate();
  const { refetch } = useTasksContext();
  const { create, loading, error } = useCreateTask((task) => {
    refetch();
    navigate(`/tasks/${task.id}/edit`, { replace: true });
  });

  return (
    <>
      <h1>New task</h1>
      <TaskForm
        submitLabel="Create task"
        loading={loading}
        error={error}
        onSubmit={async (values) => {
          await create(values);
        }}
      />
    </>
  );
}
