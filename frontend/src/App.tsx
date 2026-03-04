import { BrowserRouter, Link, Route, Routes } from 'react-router-dom';
import { TasksProvider } from './context/TasksContext';
import { HomePage } from './pages/HomePage';
import { CreateTaskPage } from './pages/CreateTaskPage';
import { EditTaskPage } from './pages/EditTaskPage';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <TasksProvider>
        <div className="app">
          <nav className="nav">
            <Link to="/">Task Tracker</Link>
            <Link to="/tasks/new">New task</Link>
          </nav>
          <main className="main">
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/tasks/new" element={<CreateTaskPage />} />
              <Route path="/tasks/:id/edit" element={<EditTaskPage />} />
            </Routes>
          </main>
        </div>
      </TasksProvider>
    </BrowserRouter>
  );
}

export default App;
