import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { createPoll } from '../services/api';
import { LoadingSpinner } from './LoadingSpinner';
import { ErrorMessage } from './ErrorMessage';
import './CreatePollScreen.css';

/**
 * Create poll screen component - allows users to create a new poll with custom options.
 */
export function CreatePollScreen() {
  const { groupId } = useParams<{ groupId: string }>();
  const navigate = useNavigate();
  
  const [question, setQuestion] = useState("Where should we eat lunch today?");
  const [options, setOptions] = useState<string[]>(['', '', '']);
  const [isCreating, setIsCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleOptionChange = (index: number, value: string) => {
    const newOptions = [...options];
    newOptions[index] = value;
    setOptions(newOptions);
  };

  const handleAddOption = () => {
    if (options.length < 10) {
      setOptions([...options, '']);
    }
  };

  const handleRemoveOption = (index: number) => {
    if (options.length > 2) {
      const newOptions = options.filter((_, i) => i !== index);
      setOptions(newOptions);
    }
  };

  const handleCreatePoll = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!groupId) {
      setError('No group ID provided.');
      return;
    }

    // Filter out empty options
    const validOptions = options.filter(opt => opt.trim() !== '');
    
    if (validOptions.length < 2) {
      setError('Please provide at least 2 options.');
      return;
    }

    if (!question.trim()) {
      setError('Please provide a question.');
      return;
    }

    try {
      setIsCreating(true);
      setError(null);
      
      await createPoll(groupId, question.trim(), validOptions);
      
      // Navigate to the poll after creation
      navigate(`/group/${groupId}`);
    } catch (err) {
      setError('Failed to create poll. Please try again.');
    } finally {
      setIsCreating(false);
    }
  };

  const handleCancel = () => {
    navigate('/');
  };

  if (isCreating) {
    return (
      <div className="create-poll-screen">
        <LoadingSpinner message="Creating poll..." />
      </div>
    );
  }

  return (
    <div className="create-poll-screen">
      <div className="create-poll-card">
        <button onClick={() => navigate('/')} className="home-button" title="Go to Home">
          🏠 Home
        </button>
        <div className="group-badge">{groupId}</div>
        <h1 className="create-poll-title">Create New Poll</h1>
        <p className="create-poll-subtitle">
          This team doesn't have a poll yet. Set one up!
        </p>

        {error && <ErrorMessage message={error} />}

        <form onSubmit={handleCreatePoll} className="create-poll-form">
          <div className="form-group">
            <label htmlFor="question" className="form-label">
              Poll Question
            </label>
            <input
              type="text"
              id="question"
              value={question}
              onChange={(e) => setQuestion(e.target.value)}
              className="question-input"
              placeholder="What do you want to ask?"
              maxLength={200}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">
              Options (add at least 2)
            </label>
            <div className="options-input-list">
              {options.map((option, index) => (
                <div key={index} className="option-input-row">
                  <input
                    type="text"
                    value={option}
                    onChange={(e) => handleOptionChange(index, e.target.value)}
                    className="option-input"
                    placeholder={`Option ${index + 1}`}
                    maxLength={100}
                  />
                  {options.length > 2 && (
                    <button
                      type="button"
                      onClick={() => handleRemoveOption(index)}
                      className="remove-option-button"
                      title="Remove option"
                    >
                      ✕
                    </button>
                  )}
                </div>
              ))}
            </div>
            
            {options.length < 10 && (
              <button
                type="button"
                onClick={handleAddOption}
                className="add-option-button"
              >
                + Add Option
              </button>
            )}
          </div>

          <div className="form-actions">
            <button
              type="button"
              onClick={handleCancel}
              className="cancel-button"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="create-button"
            >
              Create Poll
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
