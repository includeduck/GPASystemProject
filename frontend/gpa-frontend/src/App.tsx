import { useState, useEffect } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from './assets/vite.svg'
import './App.css'
import { apiService } from './services/api'

interface ApiStatus {
  test: { message: string; timestamp: string } | null;
  health: { status: string; database: string } | null;
  loading: boolean;
  error: string | null;
}

function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus>({
    test: null,
    health: null,
    loading: true,
    error: null,
  })

  useEffect(() => {
    const checkApi = async () => {
      try {
        const [testData, healthData] = await Promise.all([
          apiService.test(),
          apiService.health(),
        ])
        setApiStatus({
          test: testData,
          health: healthData,
          loading: false,
          error: null,
        })
      } catch (err) {
        setApiStatus({
          test: null,
          health: null,
          loading: false,
          error: err instanceof Error ? err.message : 'Unknown error',
        })
      }
    }

    checkApi()
  }, [])

  return (
    <>
      <div style={{ maxWidth: '800px', margin: '0 auto', padding: '20px' }}>
        <div style={{ textAlign: 'center', marginBottom: '30px' }}>
          <img src={reactLogo} alt="React logo" style={{ marginRight: '10px', display: 'inline-block' }} />
          <img src={viteLogo} alt="Vite logo" style={{ display: 'inline-block' }} />
        </div>

        <h1>🎓 GPA System</h1>
        <p>Student Result and GPA Management System</p>

        <section style={{ marginTop: '30px' }}>
          <h2>API Connection Status</h2>
          {apiStatus.loading ? (
            <p>🔄 Checking API connection...</p>
          ) : apiStatus.error ? (
            <div style={{ color: 'red', padding: '10px', backgroundColor: '#fee' }}>
              ❌ Error: {apiStatus.error}
            </div>
          ) : (
            <div style={{ color: 'green', padding: '10px', backgroundColor: '#efe' }}>
              ✅ API Connected Successfully!
            </div>
          )}

          {apiStatus.test && (
            <div style={{ marginTop: '15px', padding: '10px', backgroundColor: '#f0f0f0' }}>
              <h3>Test Endpoint</h3>
              <p><strong>Message:</strong> {apiStatus.test.message}</p>
              <p><strong>Timestamp:</strong> {apiStatus.test.timestamp}</p>
            </div>
          )}

          {apiStatus.health && (
            <div style={{ marginTop: '15px', padding: '10px', backgroundColor: '#f0f0f0' }}>
              <h3>Health Check</h3>
              <p><strong>Status:</strong> {apiStatus.health.status}</p>
              <p><strong>Database:</strong> {apiStatus.health.database}</p>
            </div>
          )}
        </section>

        <section style={{ marginTop: '30px' }}>
          <h2>Next Steps</h2>
          <ul>
            <li>✅ Backend API running at http://localhost:5273</li>
            <li>✅ Database connected (GPASystem)</li>
            <li>✅ Frontend communicating with backend</li>
            <li>⏳ Ready to build Phase 1: CRUD operations</li>
          </ul>
        </section>
      </div>
    </>
  )
}

export default App

