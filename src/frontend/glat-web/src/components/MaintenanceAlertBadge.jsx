import React, { useState, useEffect } from 'react';
import { FaExclamationTriangle, FaClock, FaCheckCircle } from 'react-icons/fa';
import { useAuthStore } from '../store';

export default function MaintenanceAlertBadge({ assetId, apiBaseUrl }) {
  const [alertCount, setAlertCount] = useState(0);
  const [requiresMaintenance, setRequiresMaintenance] = useState(false);
  const [loading, setLoading] = useState(true);
  const { token } = useAuthStore();

  useEffect(() => {
    loadMaintenanceStatus();
  }, [assetId, token]);

  const loadMaintenanceStatus = async () => {
    if (!token) {
      setLoading(false);
      return;
    }

    try {
      const baseUrl = apiBaseUrl || '/api';
      const url = `${baseUrl}/assets/${assetId}/health/maintenance`;
      
      const response = await fetch(url, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        const data = await response.json();
        setAlertCount(data.alerts?.length || 0);
        setRequiresMaintenance(data.requiresMaintenance || false);
      }
    } catch (err) {
      console.error('Failed to load maintenance status:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return null;
  }

  if (alertCount === 0) {
    return (
      <div className="mt-3 p-2 rounded-lg flex items-center gap-2 text-xs font-semibold bg-gray-50 text-gray-600 border border-gray-200">
        <FaCheckCircle className="text-gray-500" />
        <span>Maintenance up to date</span>
      </div>
    );
  }

  return (
    <div className={`mt-3 p-2 rounded-lg flex items-center gap-2 text-xs font-semibold ${
      requiresMaintenance
        ? 'bg-red-50 text-red-700 border border-red-200'
        : 'bg-yellow-50 text-yellow-700 border border-yellow-200'
    }`}>
      {requiresMaintenance ? (
        <FaExclamationTriangle className="text-red-600" />
      ) : (
        <FaClock className="text-yellow-600" />
      )}
      <span>{alertCount} maintenance alert{alertCount !== 1 ? 's' : ''}</span>
    </div>
  );
}
