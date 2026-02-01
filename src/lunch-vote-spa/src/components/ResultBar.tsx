import './ResultBar.css';

interface ResultBarProps {
  text: string;
  count: number;
  percentage: number;
  isWinner: boolean;
  highlight?: boolean;
}

/**
 * A result bar showing vote count and percentage for an option.
 */
export function ResultBar({ text, count, percentage, isWinner, highlight = false }: ResultBarProps) {
  return (
    <div className={`result-bar-container ${isWinner ? 'winner' : ''} ${highlight ? 'highlight' : ''}`}>
      <div className="result-bar-header">
        <span className="result-bar-text">
          {text}
          {isWinner && <span className="winner-badge">🏆</span>}
          {highlight && <span style={{ marginLeft: '0.5rem', color: '#0969da' }}>✓</span>}
        </span>
        <span className="result-bar-count">
          {count} {count === 1 ? 'vote' : 'votes'} ({percentage}%)
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
