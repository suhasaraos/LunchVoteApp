import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { PollResults } from '../types';
import { getPollResults, ApiRequestError } from '../services/api';
import { LoadingSpinner } from './LoadingSpinner';
import { ErrorMessage } from './ErrorMessage';
import { ResultBar } from './ResultBar';
import './ResultsScreen.css';

const AUTO_REFRESH_INTERVAL = 10000; // 10 seconds

/**
 * Results screen component - displays vote counts for a poll.
 */
export function ResultsScreen() {
  const { pollId } = useParams<{ pollId: string }>();
  const navigate = useNavigate();
  
  const [results, setResults] = useState<PollResults | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);

  const fetchResults = useCallback(async (showLoading = false) => {
    if (!pollId) {
      setError('No poll ID provided.');
      setIsLoading(false);
      return;
    }

    try {
      if (showLoading) {
        setIsLoading(true);
      }
      setError(null);
      const pollResults = await getPollResults(pollId);
      setResults(pollResults);
      setLastUpdated(new Date());
    } catch (err) {
      if (err instanceof ApiRequestError) {
        if (err.statusCode === 404) {
          setError('Poll not found.');
        } else {
          setError(err.message);
        }
      } else {
        setError('Failed to load results. Please try again.');
      }
    } finally {
      setIsLoading(false);
    }
  }, [pollId]);

  useEffect(() => {
    fetchResults(true);
  }, [fetchResults]);

  // Auto-refresh results
  useEffect(() => {
    const interval = setInterval(() => {
      fetchResults(false);
    }, AUTO_REFRESH_INTERVAL);

    return () => clearInterval(interval);
  }, [fetchResults]);

  const handleRefresh = () => {
    fetchResults(false);
  };

  const handleBackToVote = () => {
    navigate(-1);
  };

  // Find the winning option(s)
  const getWinnerIds = (): string[] => {
    if (!results || results.totalVotes === 0) return [];
    
    const maxCount = Math.max(...results.results.map(r => r.count));
    return results.results
      .filter(r => r.count === maxCount)
      .map(r => r.optionId);
  };

  if (isLoading) {
    return (
      <div className="results-screen">
        <LoadingSpinner message="Loading results..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="results-screen">
        <ErrorMessage message={error} onRetry={() => fetchResults(true)} />
      </div>
    );
  }

  if (!results) {
    return (
      <div className="results-screen">
        <ErrorMessage message="No results available." />
      </div>
    );
  }

  const winnerIds = getWinnerIds();

  return (
    <div className="results-screen">
      <div className="results-card">
        <h1 className="results-question">{results.question}</h1>
        
        <div className="results-list">
          {results.results.map((result) => (
            <ResultBar
              key={result.optionId}
              result={result}
              totalVotes={results.totalVotes}
              isWinner={winnerIds.includes(result.optionId)}
            />
          ))}
        </div>
        
        <div className="results-summary">
          <span className="total-votes">
            Total votes: <strong>{results.totalVotes}</strong>
          </span>
          {lastUpdated && (
            <span className="last-updated">
              Last updated: {lastUpdated.toLocaleTimeString()}
            </span>
          )}
        </div>
        
        <div className="results-actions">
          <button className="refresh-button" onClick={handleRefresh}>
            🔄 Refresh
          </button>
          <button className="back-button" onClick={handleBackToVote}>
            ← Back
          </button>
        </div>
      </div>
    </div>
  );
}
