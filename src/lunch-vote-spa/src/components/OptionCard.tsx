import { PollOption } from '../types';
import './OptionCard.css';

interface OptionCardProps {
  option: PollOption;
  isSelected: boolean;
  onSelect: (optionId: string) => void;
  disabled?: boolean;
}

/**
 * A voting option card that can be selected.
 */
export function OptionCard({ 
  option, 
  isSelected, 
  onSelect, 
  disabled = false 
}: OptionCardProps) {
  const handleClick = () => {
    if (!disabled) {
      onSelect(option.optionId);
    }
  };

  return (
    <button
      className={`option-card ${isSelected ? 'selected' : ''} ${disabled ? 'disabled' : ''}`}
      onClick={handleClick}
      disabled={disabled}
      type="button"
    >
      <span className="option-text">{option.text}</span>
      <span className="option-indicator">
        {isSelected ? '✓' : '○'}
      </span>
    </button>
  );
}
