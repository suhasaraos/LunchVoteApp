import { OptionResult } from '../types';
import './ResultBar.css';

interface ResultBarProps {
  result: OptionResult;
  totalVotes: number;
  isWinner: boolean;
}

/**
 * A result bar showing vote count and percentage for an option.
 */
export function ResultBar({ result, totalVotes, isWinner }: ResultBarProps) {
  const percentage = totalVotes > 0 
    ? Math.round((result.count / totalVotes) * 100) 
    : 0;

  return (
    <div className={`result-bar-container ${isWinner ? 'winner' : ''}`}>
      <div className="result-bar-header">
        <span className="result-bar-text">
          {result.text}
          {isWinner && <span className="winner-badge">🏆</span>}
        </span>
        <span className="result-bar-count">
          {result.count} {result.count === 1 ? 'vote' : 'votes'} ({percentage}%)
        </span>
      </div>
      <div className="result-bar-track">
        <div 
          className="result-bar-fill" 
          style={{ width: `${percentage}%` }}
        ></div>
      </div>
    </div>
  );
}
