import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ActivePoll } from '../types';
import { getActivePoll, submitVote, ApiRequestError } from '../services/api';
import { getOrCreateVoterToken } from '../services/voterToken';
import { LoadingSpinner } from './LoadingSpinner';
import { ErrorMessage } from './ErrorMessage';
import { OptionCard } from './OptionCard';
import './VoteScreen.css';

/**
 * Vote screen component - displays active poll for a group and allows voting.
 */
export function VoteScreen() {
  const { groupId } = useParams<{ groupId: string }>();
  const navigate = useNavigate();
  
  const [poll, setPoll] = useState<ActivePoll | null>(null);
  const [selectedOptionId, setSelectedOptionId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasVoted, setHasVoted] = useState(false);
  const [alreadyVoted, setAlreadyVoted] = useState(false);

  const fetchPoll = useCallback(async () => {
    if (!groupId) {
      setError('No group ID provided.');
      setIsLoading(false);
      return;
    }

    try {
      setIsLoading(true);
      setError(null);
      const activePoll = await getActivePoll(groupId);
      setPoll(activePoll);
    } catch (err) {
      if (err instanceof ApiRequestError) {
        if (err.statusCode === 404) {
          setError('No active poll found for this group.');
        } else {
          setError(err.message);
        }
      } else {
        setError('Failed to load poll. Please try again.');
      }
    } finally {
      setIsLoading(false);
    }
  }, [groupId]);

  useEffect(() => {
    fetchPoll();
  }, [fetchPoll]);

  const handleVote = async () => {
    if (!poll || !selectedOptionId) return;

    try {
      setIsSubmitting(true);
      setError(null);
      
      const voterToken = getOrCreateVoterToken();
      
      await submitVote({
        pollId: poll.pollId,
        optionId: selectedOptionId,
        voterToken,
      });
      
      setHasVoted(true);
    } catch (err) {
      if (err instanceof ApiRequestError) {
        if (err.error === 'AlreadyVoted') {
          setAlreadyVoted(true);
        } else {
          setError(err.message);
        }
      } else {
        setError('Failed to submit vote. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleViewResults = () => {
    if (poll) {
      navigate(`/poll/${poll.pollId}/results`);
    }
  };

  if (isLoading) {
    return (
      <div className="vote-screen">
        <LoadingSpinner message="Loading poll..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="vote-screen">
        <ErrorMessage message={error} onRetry={fetchPoll} />
      </div>
    );
  }

  if (!poll) {
    return (
      <div className="vote-screen">
        <ErrorMessage message="No poll available." />
      </div>
    );
  }

  if (hasVoted || alreadyVoted) {
    return (
      <div className="vote-screen">
        <div className="vote-success">
          <div className="success-icon">
            {hasVoted ? '🎉' : '✅'}
          </div>
          <h2 className="success-title">
            {hasVoted ? 'Thank you for voting!' : 'You have already voted'}
          </h2>
          <p className="success-message">
            {hasVoted 
              ? 'Your vote has been recorded successfully.'
              : 'You can only vote once per poll from this device.'}
          </p>
          <button className="view-results-button" onClick={handleViewResults}>
            View Results
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="vote-screen">
      <div className="vote-card">
        <div className="group-badge">{poll.groupId}</div>
        <h1 className="poll-question">{poll.question}</h1>
        
        <div className="options-list">
          {poll.options.map((option) => (
            <OptionCard
              key={option.optionId}
              option={option}
              isSelected={selectedOptionId === option.optionId}
              onSelect={setSelectedOptionId}
              disabled={isSubmitting}
            />
          ))}
        </div>
        
        <button
          className="submit-button"
          onClick={handleVote}
          disabled={!selectedOptionId || isSubmitting}
        >
          {isSubmitting ? 'Submitting...' : 'Submit Vote'}
        </button>
      </div>
    </div>
  );
}
