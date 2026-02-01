import './ErrorMessage.css';

interface ErrorMessageProps {
  title?: string;
  message: string;
  onRetry?: () => void;
}

/**
 * Error message component for displaying API errors.
 */
export function ErrorMessage({ 
  title = 'Error', 
  message, 
  onRetry 
}: ErrorMessageProps) {
  return (
    <div className="error-container">
      <div className="error-icon">⚠️</div>
      <h2 className="error-title">{title}</h2>
      <p className="error-message">{message}</p>
      {onRetry && (
        <button className="retry-button" onClick={onRetry}>
          Try Again
        </button>
      )}
    </div>
  );
}
