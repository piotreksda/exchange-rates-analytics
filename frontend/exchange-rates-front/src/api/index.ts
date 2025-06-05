export * from './types';
export * from './config';
export * from './utils';
export * from './exchangeRatesApi';

// Create a default API instance for convenience
import { ExchangeRatesApiService } from './exchangeRatesApi';

const api = new ExchangeRatesApiService();
export default api;
