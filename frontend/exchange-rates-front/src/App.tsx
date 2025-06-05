import './App.css'
import { Button } from '@/components/ui/button'
import { useState } from 'react'
import api, { ChartMode, ChartTimeWindow } from './api'

function App() {
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<string | null>(null)

  const handleConvert = async () => {
    setLoading(true)
    try {
      const response = await api.convert('USD', 'EUR', 100)
      setResult(`Converted ${response.fromCurrency} to ${response.toCurrency}: ${response.convertedAmount}`)
    } catch (error) {
      setResult(`Error: ${error instanceof Error ? error.message : String(error)}`)
    } finally {
      setLoading(false)
    }
  }

  const handleGetChart = async () => {
    setLoading(true)
    try {
      const response = await api.getChartData(
        'USD',
        ['EUR', 'GBP'],
        ChartMode.BUY,
        ChartTimeWindow.MONTH,
        '2025-05-01',
        '2025-06-01'
      )
      setResult(`Chart data: ${JSON.stringify(response, null, 2)}`)
    } catch (error) {
      setResult(`Error: ${error instanceof Error ? error.message : String(error)}`)
    } finally {
      setLoading(false)
    }
  }

  const handleMigrate = async () => {
    setLoading(true)
    try {
      await api.migrate()
      setResult('Migration completed successfully')
    } catch (error) {
      setResult(`Error: ${error instanceof Error ? error.message : String(error)}`)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-svh flex-col items-center justify-center gap-4 p-4">
      <h1 className="text-2xl font-bold mb-4">Exchange Rates API Demo</h1>
      
      <div className="flex gap-2">
        <Button onClick={handleConvert} disabled={loading}>Convert Currency</Button>
        <Button onClick={handleGetChart} disabled={loading}>Get Chart Data</Button>
        <Button onClick={handleMigrate} disabled={loading}>Migrate Data</Button>
      </div>
      
      {loading && <p>Loading...</p>}
      
      {result && (
        <div className="mt-4 p-4 border rounded bg-gray-50 max-w-lg overflow-auto">
          <pre className="whitespace-pre-wrap">{result}</pre>
        </div>
      )}
    </div>
  )
}

export default App
