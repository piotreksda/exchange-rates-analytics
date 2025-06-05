// API utilities for handling responses and errors
import {
  CHART_MODE_VALUES,
  CHART_TIME_WINDOW_VALUES,
  ChartMode,
  ChartTimeWindow,
} from './types';

/**
 * Formats query parameters for URL
 * Creates a URL query string from the provided parameters
 * @param params Object with parameters to convert to query string
 */
export const formatQueryParams = <T extends Record<string, unknown>>(params: T): string => {
  const queryParams = new URLSearchParams();
  
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null) {
      if (Array.isArray(value)) {
        // Handle array parameters (like to[] in chart endpoint)
        value.forEach((item) => {
          queryParams.append(`${key}`, String(item));
        });
      } else if (key === 'chartMode' && typeof value === 'number') {
        // Handle ChartMode enum
        queryParams.append(key, CHART_MODE_VALUES[value as ChartMode]);
      } else if (key === 'chartTimeWindow' && typeof value === 'number') {
        // Handle ChartTimeWindow enum
        queryParams.append(key, CHART_TIME_WINDOW_VALUES[value as ChartTimeWindow]);
      } else {
        queryParams.append(key, String(value));
      }
    }
  });
  
  const queryString = queryParams.toString();
  return queryString ? `?${queryString}` : '';
};

/**
 * Handles API response and errors
 */
export const handleApiResponse = async <T>(response: Response): Promise<T> => {
  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(
      `API Error (${response.status}): ${
        errorText || response.statusText || 'Unknown error'
      }`
    );
  }
  
  // Check if there's content to parse
  const contentType = response.headers.get('content-type');
  if (contentType && contentType.includes('application/json')) {
    return await response.json() as T;
  }
  
  return {} as T; // Return empty object for responses with no content
};
