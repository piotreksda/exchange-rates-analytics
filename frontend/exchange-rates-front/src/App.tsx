import './App.css'
import { useState } from 'react'
import { format } from 'date-fns'
import { Calendar as CalendarIcon } from 'lucide-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts'
import type { DateRange } from "react-day-picker"
import api, { ChartMode, ChartTimeWindow, CHART_MODE_VALUES, CHART_TIME_WINDOW_VALUES } from './api'
import type { ChartResponse } from './api'

// Import shadcn components
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent } from '@/components/ui/card'
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { Calendar } from '@/components/ui/calendar'

function App() {
  // State for form values
  const [amount, setAmount] = useState<number>(100)
  const [fromCurrency, setFromCurrency] = useState<string>('USD')
  const [toCurrency, setToCurrency] = useState<string>('EUR')
  const [selectedCurrencies, setSelectedCurrencies] = useState<string[]>(['EUR'])
  const [chartMode, setChartMode] = useState<ChartMode>(ChartMode.BUY)
  const [timeWindow, setTimeWindow] = useState<ChartTimeWindow>(ChartTimeWindow.MONTH)
  
  // State for date range
  const [dateRange, setDateRange] = useState<DateRange | undefined>({
    from: new Date(2010, 0, 1), // January 1, 2010
    to: new Date(), // Current date
  })

  // State for API results
  const [loading, setLoading] = useState<boolean>(false)
  const [result, setResult] = useState<string | null>(null)
  const [chartData, setChartData] = useState<Array<{name: string, value: number}>>([])

  // Currency options
  const currencies = ["USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "PLN"]
  
  // Handler for single currency conversion
  const handleConvert = async () => {
    if (!amount || !fromCurrency || !toCurrency) return
    
    setLoading(true)
    try {
      const response = await api.convert(fromCurrency, toCurrency, amount)
      setResult(`Converted ${response.fromCurrency} to ${response.toCurrency}: ${response.convertedAmount}`)
      
      // Mock chart data for visualization
      const mockData = [
        { name: 'Jan', value: 100 },
        { name: 'Feb', value: 120 },
        { name: 'Mar', value: 110 },
        { name: 'Apr', value: 130 },
        { name: 'May', value: 90 },
      ]
      setChartData(mockData)
    } catch (error) {
      setResult(`Error: ${error instanceof Error ? error.message : String(error)}`)
    } finally {
      setLoading(false)
    }
  }

  // Handler for chart data
  const handleGetChart = async () => {
    if (!fromCurrency || selectedCurrencies.length === 0) return
    
    setLoading(true)
    try {
      const from = dateRange?.from ? format(dateRange.from, 'yyyy-MM-dd') : undefined
      const to = dateRange?.to ? format(dateRange.to, 'yyyy-MM-dd') : undefined
      
      const response = await api.getChartData(
        fromCurrency,
        selectedCurrencies,
        chartMode,
        timeWindow,
        from,
        to
      )
      
      setResult(`Chart data: ${JSON.stringify(response, null, 2)}`)
      
      // Transform the response data for the chart
      // Assuming response is an array of ChartResponse
      const chartDataFormatted = Array.isArray(response) ? response.map((item: ChartResponse) => ({
        name: item.label,
        value: Object.values(item.data)[0] as number
      })) : []
      
      setChartData(chartDataFormatted)
    } catch (error) {
      setResult(`Error: ${error instanceof Error ? error.message : String(error)}`)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-svh flex-col items-center justify-center gap-6 p-6">
      <h1 className="text-2xl font-bold mb-2">Exchange Rates Analytics</h1>
      
      {/* Simple Conversion Card */}
      <Card className="w-full max-w-4xl">
        <CardContent className="p-6">
          <div className="flex flex-col gap-4">
            <div className="flex flex-wrap gap-4 items-end">
              {/* Amount Input */}
              <div className="w-full md:w-auto">
                <Label htmlFor="amount">Number Input</Label>
                <Input 
                  id="amount"
                  type="number"
                  value={amount}
                  onChange={(e) => setAmount(Number(e.target.value))}
                  className="w-full"
                  placeholder="Enter amount"
                />
              </div>
              
              <div className="flex flex-col gap-1">
                <span className="text-sm text-muted-foreground">Selects</span>
                <div className="flex gap-2">
                  {/* From Currency */}
                  <div>
                    <Select 
                      value={fromCurrency} 
                      onValueChange={setFromCurrency}
                    >
                      <SelectTrigger className="w-[120px]">
                        <SelectValue placeholder="From" />
                      </SelectTrigger>
                      <SelectContent>
                        {currencies.map(currency => (
                          <SelectItem key={currency} value={currency}>
                            {currency}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <Label className="mt-1 block text-center text-xs">From Currency</Label>
                  </div>
                  
                  {/* To Currency */}
                  <div>
                    <Select 
                      value={toCurrency} 
                      onValueChange={setToCurrency}
                    >
                      <SelectTrigger className="w-[120px]">
                        <SelectValue placeholder="To" />
                      </SelectTrigger>
                      <SelectContent>
                        {currencies.map(currency => (
                          <SelectItem key={currency} value={currency}>
                            {currency}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <Label className="mt-1 block text-center text-xs">To Currency</Label>
                  </div>
                </div>
              </div>
              
              <Button 
                onClick={handleConvert} 
                className="ml-auto"
                disabled={loading}
              >
                Convert
              </Button>
            </div>
            
            {/* Conversion Result Display */}
            <div className="mt-2">
              <div className="h-[120px] border rounded-md p-4 flex items-center justify-center">
                <p className="text-center text-muted-foreground">1 {fromCurrency} currently is {amount} {toCurrency}</p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
      
      {/* Advanced Options Card */}
      <Card className="w-full max-w-8xl">
        <CardContent className="p-6">
          <div className="flex flex-col gap-4">
            <div className="grid grid-cols-1 md:grid-cols-5 gap-6">
              {/* Multiple From Currency */}
              <div>
                <Label className="block mb-2">From Currency</Label>
                <Select 
                  value={fromCurrency} 
                  onValueChange={setFromCurrency}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="From" />
                  </SelectTrigger>
                  <SelectContent>
                    {currencies.map(currency => (
                      <SelectItem key={currency} value={currency}>
                        {currency}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              
              {/* Multiple To Currency */}
              <div>
                <Label className="block mb-2">To Currency</Label>
                <Select 
                  value={selectedCurrencies[0]} 
                  onValueChange={(value) => setSelectedCurrencies([value])}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="To" />
                  </SelectTrigger>
                  <SelectContent>
                    {currencies.filter(c => c !== fromCurrency).map(currency => (
                      <SelectItem key={currency} value={currency}>
                        {currency}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              
              {/* Buy/Sell Switch */}
              <div>
                <Label className="block mb-2">Switch Buy or Sell</Label>
                <Tabs 
                  value={chartMode.toString()}
                  onValueChange={(value) => setChartMode(Number(value) as ChartMode)}
                  className="w-full"
                >
                  <TabsList className="w-full grid grid-cols-2">
                    <TabsTrigger value={ChartMode.BUY.toString()}>{CHART_MODE_VALUES[ChartMode.BUY]}</TabsTrigger>
                    <TabsTrigger value={ChartMode.SELL.toString()}>{CHART_MODE_VALUES[ChartMode.SELL]}</TabsTrigger>
                  </TabsList>
                </Tabs>
              </div>
              
              {/* Time Window Select */}
              <div>
                <Label className="block mb-2">Time Window Select</Label>
                <Select 
                  value={timeWindow.toString()} 
                  onValueChange={(value) => setTimeWindow(Number(value) as ChartTimeWindow)}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Time Window" />
                  </SelectTrigger>
                  <SelectContent>
                    {Object.entries(CHART_TIME_WINDOW_VALUES).map(([value, label]) => (
                      <SelectItem key={value} value={value}>
                        {label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              
              {/* Calendar Date Range */}
              <div>
                <Label className="block mb-2">Date Range</Label>
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      className="w-full justify-start text-left font-normal"
                    >
                      <CalendarIcon className="mr-2 h-4 w-4" />
                      {dateRange?.from ? (
                        dateRange.to ? (
                          <>
                            {format(dateRange.from, "LLL dd, y")} - {format(dateRange.to, "LLL dd, y")}
                          </>
                        ) : (
                          format(dateRange.from, "LLL dd, y")
                        )
                      ) : (
                        <span>Pick a date range</span>
                      )}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0" align="start">
                    <Calendar
                      mode="range"
                      selected={dateRange}
                      onSelect={(range) => setDateRange(range)}
                      initialFocus
                      className="rounded-md border"
                    />
                  </PopoverContent>
                </Popover>
              </div>
            </div>
            
            <Button 
              onClick={handleGetChart} 
              className="ml-auto"
              disabled={loading}
            >
              Get Chart Data
            </Button>              {/* Chart Display */}
            <div className="mt-2">
              <div className="h-[300px] border rounded-md p-4">
                {chartData.length > 0 ? (
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart 
                      data={chartData}
                      margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                    >
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis 
                        dataKey="name" 
                        label={{ value: 'Date', position: 'insideBottomRight', offset: -10 }}
                      />
                      <YAxis 
                        label={{ value: 'Rate', angle: -90, position: 'insideLeft' }}
                      />
                      <Tooltip />
                      <Legend verticalAlign="top" height={36} />
                      <Line 
                        name={`${fromCurrency} to ${selectedCurrencies[0]}`}
                        type="monotone" 
                        dataKey="value" 
                        stroke="#8884d8" 
                        strokeWidth={2}
                        activeDot={{ r: 8 }}
                      />
                    </LineChart>
                  </ResponsiveContainer>
                ) : (
                  <p className="text-center text-muted-foreground">Chart data will appear here</p>
                )}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
      
      {/* Loading and Result States */}
      {loading && <p className="text-center">Loading...</p>}
      
      {result && !loading && (
        <div className="mt-2 p-4 border rounded bg-gray-50 max-w-lg overflow-auto hidden">
          <pre className="whitespace-pre-wrap">{result}</pre>
        </div>
      )}
    </div>
  )
}

export default App
