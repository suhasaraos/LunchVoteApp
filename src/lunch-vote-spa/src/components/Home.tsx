import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { getAllGroups } from '../services/api';
import './Home.css';

/**
 * Home page component - allows users to enter a group ID to vote.
 */
export function Home() {
  const [groupId, setGroupId] = useState('');
  const [availableGroups, setAvailableGroups] = useState<string[]>([]);
  const navigate = useNavigate();
  const location = useLocation();

  const fetchGroups = async () => {
    try {
      const groups = await getAllGroups();
      setAvailableGroups(groups);
    } catch (error) {
      // If fetching fails, use default groups
      setAvailableGroups(['platform', 'security', 'apps', 'data']);
    }
  };

  useEffect(() => {
    fetchGroups();
    
    // Also refetch when window regains focus (user returns to tab/window)
    const handleFocus = () => {
      fetchGroups();
    };
    
    window.addEventListener('focus', handleFocus);
    return () => window.removeEventListener('focus', handleFocus);
  }, [location]); // Refetch whenever location changes (navigating back to home)

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (groupId.trim()) {
      navigate(`/group/${groupId.trim().toLowerCase()}`);
    }
  };

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
            {availableGroups.length > 0 ? (
              availableGroups.map((group) => (
                <button
                  key={group}
                  type="button"
                  className="group-chip"
                  onClick={() => navigate(`/group/${group}`)}
                >
                  {group}
                </button>
              ))
            ) : (
              <p style={{ color: '#666', fontSize: '0.9rem' }}>No teams yet. Create one above!</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
