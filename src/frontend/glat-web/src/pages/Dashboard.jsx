import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaExclamationTriangle } from 'react-icons/fa';
import { useAssetStore, useHealthStore, useAuthStore } from '../store';
import { assetService, healthService } from '../services/apiClient';
import CreateAssetModal from '../components/CreateAssetModal';
import MaintenanceAlertBadge from '../components/MaintenanceAlertBadge';

export default function Dashboard() {
  const navigate = useNavigate();
  const { assets, isLoading, error, setAssets, setLoading, setError } = useAssetStore();
  const { systemHealth, setSystemHealth, setLoading: setHealthLoading } = useHealthStore();
  const { token } = useAuthStore();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [assetsRequiringMaintenance, setAssetsRequiringMaintenance] = useState([]);

  useEffect(() => {
    loadAssets();
    loadSystemHealth();
  }, []);

  useEffect(() => {
    if (assets.length > 0 && token) {
      checkMaintenanceStatus();
    }
  }, [assets, token]);

  const loadAssets = async () => {
    try {
      setLoading(true);
      const response = await assetService.getAssets();
      setAssets(response.data.items);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const loadSystemHealth = async () => {
    try {
      setHealthLoading(true);
      const response = await healthService.getSystemHealth();
      setSystemHealth(response.data);
    } catch (err) {
      console.error('Failed to load system health');
    } finally {
      setHealthLoading(false);
    }
  };

  const checkMaintenanceStatus = async () => {
    const baseUrl = '/api';
    const requireingMaintenance = [];

    for (const asset of assets) {
      try {
        const response = await fetch(
          `${baseUrl}/assets/${asset.id}/health/maintenance`,
          {
            headers: {
              'Authorization': `Bearer ${token}`,
              'Content-Type': 'application/json'
            }
          }
        );

        if (response.ok) {
          const data = await response.json();
          if (data.requiresMaintenance) {
            requireingMaintenance.push({
              ...asset,
              alerts: data.alerts
            });
          }
        }
      } catch (err) {
        console.error(`Failed to check maintenance for asset ${asset.id}`);
      }
    }

    setAssetsRequiringMaintenance(requireingMaintenance);
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'ACTIVE':
        return 'bg-green-100 text-green-800';
      case 'INACTIVE':
        return 'bg-gray-100 text-gray-800';
      case 'MAINTENANCE':
        return 'bg-yellow-100 text-yellow-800';
      case 'RETIRED':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 py-6">
          <h1 className="text-3xl font-bold text-gray-900">Global Logistics Asset Tracker</h1>
          <p className="mt-2 text-sm text-gray-600">Monitor and manage your logistics assets in real-time</p>
        </div>
      </div>

      {/* System Health */}
      {systemHealth && (
        <div className="max-w-7xl mx-auto px-4 py-4">
          <div className={`p-4 rounded-lg ${
            systemHealth.status === 'HEALTHY'
              ? 'bg-green-50 border border-green-200'
              : systemHealth.status === 'DEGRADED'
              ? 'bg-yellow-50 border border-yellow-200'
              : 'bg-red-50 border border-red-200'
          }`}>
            <p className="text-sm font-semibold">System Status: {systemHealth.status}</p>
            <p className="text-xs text-gray-600 mt-1">Database: {systemHealth.database?.status || 'Unknown'}</p>
          </div>
        </div>
      )}

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 py-8">
        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-sm text-red-800">{error}</p>
          </div>
        )}

        {/* Assets Requiring Immediate Maintenance */}
        {assetsRequiringMaintenance.length > 0 && (
          <div className="mb-8 p-4 bg-red-50 border-l-4 border-red-500 rounded-lg">
            <div className="flex items-start gap-3">
              <FaExclamationTriangle className="text-red-600 mt-1" />
              <div>
                <h3 className="font-semibold text-red-900 mb-2">
                  {assetsRequiringMaintenance.length} Asset{assetsRequiringMaintenance.length !== 1 ? 's' : ''} Require Immediate Maintenance
                </h3>
                <div className="space-y-1">
                  {assetsRequiringMaintenance.map((asset) => (
                    <button
                      key={asset.id}
                      onClick={() => navigate(`/assets/${asset.id}`)}
                      className="block text-red-700 hover:text-red-900 hover:underline text-sm"
                    >
                      • {asset.serialNumber} ({asset.type}) - {asset.alerts?.length || 0} alert{asset.alerts?.length !== 1 ? 's' : ''}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Assets Header with Create Button */}
        <div className="mb-6 flex justify-between items-center">
          <h2 className="text-2xl font-bold text-gray-900">Assets</h2>
          <button
            onClick={() => setIsCreateModalOpen(true)}
            className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition font-medium flex items-center gap-2"
          >
            <span>+</span> Create Asset
          </button>
        </div>

        {/* Assets Grid or Empty State */}
        {isLoading ? (
          <div className="text-center py-12 text-gray-600">Loading assets...</div>
        ) : assets.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-gray-600 mb-4">No assets found. Create one to get started.</p>
          </div>
        ) : (
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {assets.map((asset) => (
              <div key={asset.id} className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition">
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">{asset.serialNumber}</h3>
                    <p className="text-sm text-gray-500">{asset.type}</p>
                  </div>
                  <span className={`px-3 py-1 rounded-full text-xs font-semibold ${getStatusColor(asset.status)}`}>
                    {asset.status}
                  </span>
                </div>

                <div className="space-y-2 text-sm">
                  <p className="text-gray-700">
                    <span className="font-semibold">Location:</span> {asset.location}
                  </p>
                  {asset.owner && (
                    <p className="text-gray-700">
                      <span className="font-semibold">Owner:</span> {asset.owner}
                    </p>
                  )}
                  {asset.description && (
                    <p className="text-gray-600 line-clamp-2">{asset.description}</p>
                  )}
                </div>

                {/* Maintenance Alert Badge */}
                <MaintenanceAlertBadge 
                  assetId={asset.id}
                  apiBaseUrl="/api"
                />

                <div className="mt-4 pt-4 border-t border-gray-200">
                  <button 
                    onClick={() => navigate(`/assets/${asset.id}`)}
                    className="w-full px-3 py-2 bg-blue-600 text-white text-sm rounded hover:bg-blue-700 transition"
                  >
                    View Details
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Create Asset Modal */}
      <CreateAssetModal 
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onAssetCreated={() => {
          loadAssets();
        }}
      />
    </div>
  );
}
