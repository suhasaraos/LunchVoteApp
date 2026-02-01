import './LoadingSpinner.css';

interface LoadingSpinnerProps {
  message?: string;
}

/**
 * Loading spinner component displayed during API calls.
 */
export function LoadingSpinner({ message = 'Loading...' }: LoadingSpinnerProps) {
  return (
    <div className="loading-container">
      <div className="loading-spinner"></div>
      <p className="loading-message">{message}</p>
    </div>
  );
}
