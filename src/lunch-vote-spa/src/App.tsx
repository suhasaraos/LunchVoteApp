import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Home } from './components/Home';
import { VoteScreen } from './components/VoteScreen';
import { ResultsScreen } from './components/ResultsScreen';
import { CreatePollScreen } from './components/CreatePollScreen';
import './App.css';

/**
 * Main application component with routing.
 */
function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/group/:groupId" element={<VoteScreen />} />
        <Route path="/group/:groupId/create" element={<CreatePollScreen />} />
        <Route path="/poll/:pollId/results" element={<ResultsScreen />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
