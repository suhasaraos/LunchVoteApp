import { ActivePoll, ApiError, PollResults, VoteRequest, VoteResponse } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

/**
 * Custom error class for API errors.
 */
export class ApiRequestError extends Error {
  public error: string;
  public statusCode: number;

  constructor(error: string, message: string, statusCode: number) {
    super(message);
    this.error = error;
    this.statusCode = statusCode;
    this.name = 'ApiRequestError';
  }
}

/**
 * Handles API responses and throws appropriate errors.
 */
async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let errorData: ApiError;
    
    try {
      errorData = await response.json();
    } catch {
      errorData = {
        error: 'UnknownError',
        message: 'An unexpected error occurred.',
      };
    }
    
    throw new ApiRequestError(errorData.error, errorData.message, response.status);
  }
  
  return response.json();
}

/**
 * Gets the active poll for a group.
 */
export async function getActivePoll(groupId: string): Promise<ActivePoll> {
  const response = await fetch(
    `${API_BASE_URL}/polls/active?groupId=${encodeURIComponent(groupId)}`
  );
  
  return handleResponse<ActivePoll>(response);
}

/**
 * Submits a vote for a poll.
 */
export async function submitVote(request: VoteRequest): Promise<VoteResponse> {
  const response = await fetch(`${API_BASE_URL}/votes`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });
  
  return handleResponse<VoteResponse>(response);
}

/**
 * Gets the results for a poll.
 */
export async function getPollResults(pollId: string): Promise<PollResults> {
  const response = await fetch(`${API_BASE_URL}/polls/${pollId}/results`);
  
  return handleResponse<PollResults>(response);
}

/**
 * Creates a new poll.
 */
export async function createPoll(
  groupId: string,
  question: string,
  options: string[]
): Promise<{ pollId: string }> {
  const response = await fetch(`${API_BASE_URL}/polls`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ groupId, question, options }),
  });
  
  return handleResponse<{ pollId: string }>(response);
}
