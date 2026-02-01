/**
 * An active poll with its voting options.
 */
export interface ActivePoll {
  pollId: string;
  groupId: string;
  question: string;
  options: PollOption[];
}

/**
 * A voting option within a poll.
 */
export interface PollOption {
  optionId: string;
  text: string;
}

/**
 * Request to submit a vote.
 */
export interface VoteRequest {
  pollId: string;
  optionId: string;
  voterToken: string;
}

/**
 * Poll results with vote counts.
 */
export interface PollResults {
  pollId: string;
  question: string;
  results: OptionResult[];
  totalVotes: number;
}

/**
 * Vote count for a single option.
 */
export interface OptionResult {
  optionId: string;
  text: string;
  count: number;
}

/**
 * API error response.
 */
export interface ApiError {
  error: string;
  message: string;
}

/**
 * Vote submission response.
 */
export interface VoteResponse {
  message: string;
}
