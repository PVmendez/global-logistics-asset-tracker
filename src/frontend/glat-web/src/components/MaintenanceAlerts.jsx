import React, { useState, useEffect } from 'react';
import { FaExclamationTriangle, FaCheckCircle, FaClock } from 'react-icons/fa';
import { useAuthStore } from '../store';

export default function MaintenanceAlerts({ assetId, apiBaseUrl }) {
  const [alerts, setAlerts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const { token } = useAuthStore();

  useEffect(() => {
    loadMaintenance();
  }, [assetId, token]);

  const loadMaintenance = async () => {
    if (!token) {
      setError('Not authenticated');
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      const baseUrl = apiBaseUrl || '/api';
      const url = `${baseUrl}/assets/${assetId}/health/maintenance`;
      
      const response = await fetch(url, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        if (response.status === 401) {
          throw new Error('Unauthorized - Please login again');
        }
        throw new Error(`Failed to load maintenance data (${response.status})`);
      }

      const data = await response.json();
      setAlerts(data.alerts || []);
      setError(null);
    } catch (err) {
      setError(err.message);
      setAlerts([]);
    } finally {
      setLoading(false);
    }
  };

  const getSeverityColor = (severity) => {
    switch (severity) {
      case 'CRITICAL':
        return 'bg-red-50 border-l-4 border-red-500 text-red-800';
      case 'WARNING':
        return 'bg-yellow-50 border-l-4 border-yellow-500 text-yellow-800';
      case 'INFO':
        return 'bg-blue-50 border-l-4 border-blue-500 text-blue-800';
      default:
        return 'bg-gray-50 border-l-4 border-gray-500 text-gray-800';
    }
  };

  const getAlertIcon = (alertType) => {
    switch (alertType) {
      case 'OVERDUE':
        return <FaExclamationTriangle className="mr-3" />;
      case 'SCHEDULED':
        return <FaClock className="mr-3" />;
      case 'LOW_HEALTH':
        return <FaExclamationTriangle className="mr-3" />;
      default:
        return <FaCheckCircle className="mr-3" />;
    }
  };

  if (loading) {
    return (
      <div className="bg-gray-50 rounded-lg p-4">
        <p className="text-gray-500 text-sm">Loading maintenance data...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 rounded-lg p-4 border border-red-200">
        <p className="text-red-600 text-sm font-medium">Error: {error}</p>
        <p className="text-red-500 text-xs mt-1">Please ensure you are logged in and the API is accessible.</p>
      </div>
    );
  }

  if (!alerts || alerts.length === 0) {
    return (
      <div className="bg-green-50 rounded-lg p-4 border border-green-200">
        <div className="flex items-center text-green-800">
          <FaCheckCircle className="mr-2" />
          <span className="text-sm font-medium">No maintenance alerts</span>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {alerts.map((alert, index) => (
        <div
          key={index}
          className={`rounded-lg p-4 flex items-start ${getSeverityColor(alert.severity)}`}
        >
          <div className="flex-shrink-0 mt-0.5">
            {getAlertIcon(alert.alertType)}
          </div>
          <div className="flex-1">
            <h4 className="font-semibold text-sm">{alert.alertType.replace('_', ' ')}</h4>
            <p className="text-sm mt-1">{alert.message}</p>
            {alert.dueDate && (
              <p className="text-xs mt-2 opacity-75">
                Due: {new Date(alert.dueDate).toLocaleDateString()}
              </p>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
