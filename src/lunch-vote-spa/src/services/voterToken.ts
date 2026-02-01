const STORAGE_KEY = 'voterToken';

/**
 * Gets the voter token from localStorage, or creates and stores a new one.
 * The token is a UUID that uniquely identifies this browser/device.
 */
export function getOrCreateVoterToken(): string {
  let token = localStorage.getItem(STORAGE_KEY);
  
  if (!token) {
    token = crypto.randomUUID();
    localStorage.setItem(STORAGE_KEY, token);
  }
  
  return token;
}

/**
 * Clears the voter token from localStorage.
 * Useful for testing or allowing a user to vote again.
 */
export function clearVoterToken(): void {
  localStorage.removeItem(STORAGE_KEY);
}
