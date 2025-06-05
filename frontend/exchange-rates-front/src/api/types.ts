// API types for exchange rates service

// Chart modes
export const ChartMode = {
  BUY: 0,
  SELL: 1
} as const;

export type ChartMode = typeof ChartMode[keyof typeof ChartMode];

// String representations of chart modes
export const CHART_MODE_VALUES = {
  [ChartMode.BUY]: "Buy",
  [ChartMode.SELL]: "Sell",
} as const;

// Chart time window
export const ChartTimeWindow = {
  DAY: 0,
  QUARTER: 1,
  MONTH: 2,
  YEAR: 3,
} as const;

export type ChartTimeWindow = typeof ChartTimeWindow[keyof typeof ChartTimeWindow];

// String representations of chart time windows
export const CHART_TIME_WINDOW_VALUES = {
  [ChartTimeWindow.DAY]: "Day",
  [ChartTimeWindow.QUARTER]: "Quarter",
  [ChartTimeWindow.MONTH]: "Month",
  [ChartTimeWindow.YEAR]: "Year",
} as const;

// Response type for convert endpoint
export interface ConvertResponse {
  convertedAmount: number;
  fromCurrency: string;
  toCurrency: string;
}

// Response type for chart endpoint
export interface ChartResponse {
  label: string;
  data: Record<string, number>;
}

// Chart request parameters
export interface ChartParams {
  from: string;
  to: string[];
  chartMode: ChartMode;
  startDate?: string; // Format: YYYY-MM-DD
  endDate?: string; // Format: YYYY-MM-DD
  chartTimeWindow: ChartTimeWindow;
}

// Convert request parameters
export interface ConvertParams {
  from: string;
  to: string;
  amount: number;
}
