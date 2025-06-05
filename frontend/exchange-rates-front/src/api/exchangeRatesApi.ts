import { API_CONFIG } from './config';
import { formatQueryParams, handleApiResponse } from './utils';
import type {
    ChartMode,
    ChartParams,
    ChartResponse,
    ChartTimeWindow,
    ConvertParams,
    ConvertResponse,
} from './types';

/**
 * Exchange Rates API Service
 * Provides methods to interact with the exchange rates API
 */
export class ExchangeRatesApiService {
  private baseUrl: string;

  constructor(baseUrl?: string) {
    this.baseUrl = baseUrl || API_CONFIG.baseUrl;
  }

  /**
   * Triggers data migration on the server
   * @returns Promise that resolves when migration is complete
   */
  public async migrate(): Promise<void> {
    const url = `${this.baseUrl}/api/migrate`;
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    await handleApiResponse<void>(response);
  }

  /**
   * Converts an amount from one currency to another
   * @param from The source currency code
   * @param to The target currency code
   * @param amount The amount to convert
   * @returns The converted amount and currency information
   */
  public async convert(from: string, to: string, amount: number): Promise<ConvertResponse> {
    const params: ConvertParams = { from, to, amount };
    // Use spread operator to convert to Record<string, unknown>
    const queryString = formatQueryParams({ ...params });
    const url = `${this.baseUrl}/api/convert${queryString}`;
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    return await handleApiResponse<ConvertResponse>(response);
  }

  /**
   * Gets chart data for currency rates
   * @param from The source currency code
   * @param to Array of target currency codes
   * @param chartMode The chart mode (buy or sell)
   * @param chartTimeWindow The time window for chart data
   * @param startDate Optional start date (format: YYYY-MM-DD)
   * @param endDate Optional end date (format: YYYY-MM-DD)
   * @returns Chart data for the specified currencies and time period
   */
  public async getChartData(
    from: string,
    to: string[],
    chartMode: ChartMode,
    chartTimeWindow: ChartTimeWindow,
    startDate?: string,
    endDate?: string
  ): Promise<ChartResponse> {
    const params: ChartParams = {
      from,
      to,
      chartMode,
      chartTimeWindow,
      startDate,
      endDate
    };
    // Use spread operator to convert to Record<string, unknown>
    const queryString = formatQueryParams({ ...params });
    const url = `${this.baseUrl}/api/chart${queryString}`;
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    return await handleApiResponse<ChartResponse>(response);
  }
}
