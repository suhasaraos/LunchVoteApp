import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './Home.css';

/**
 * Home page component - allows users to enter a group ID to vote.
 */
export function Home() {
  const [groupId, setGroupId] = useState('');
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (groupId.trim()) {
      navigate(`/group/${groupId.trim().toLowerCase()}`);
    }
  };

  const popularGroups = ['platform', 'security', 'apps', 'data'];

  return (
    <div className="home-screen">
      <div className="home-card">
        <div className="home-header">
          <span className="home-emoji">🍕</span>
          <h1 className="home-title">Lunch Vote</h1>
          <p className="home-subtitle">
            Vote for your team's lunch destination
          </p>
        </div>

        <form onSubmit={handleSubmit} className="group-form">
          <label htmlFor="groupId" className="form-label">
            Enter your team name
          </label>
          <input
            type="text"
            id="groupId"
            value={groupId}
            onChange={(e) => setGroupId(e.target.value)}
            placeholder="e.g., platform"
            className="group-input"
            maxLength={50}
          />
          <button
            type="submit"
            className="go-button"
            disabled={!groupId.trim()}
          >
            Go to Poll →
          </button>
        </form>

        <div className="popular-groups">
          <p className="popular-label">Or select a team:</p>
          <div className="group-buttons">
            {popularGroups.map((group) => (
              <button
                key={group}
                type="button"
                className="group-chip"
                onClick={() => navigate(`/group/${group}`)}
              >
                {group}
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
