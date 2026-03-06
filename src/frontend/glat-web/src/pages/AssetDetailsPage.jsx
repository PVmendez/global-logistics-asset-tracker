import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { FaBox, FaTruck, FaCog, FaClipboard } from 'react-icons/fa';
import { assetService } from '../services/apiClient';
import MaintenanceAlerts from '../components/MaintenanceAlerts';

export default function AssetDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [asset, setAsset] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadAsset();
  }, [id]);

  const loadAsset = async () => {
    try {
      setLoading(true);
      const response = await assetService.getAssetById(id);
      setAsset(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to load asset details');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete this asset?')) {
      try {
        await assetService.deleteAsset(id);
        navigate('/dashboard');
      } catch (err) {
        alert('Failed to delete asset');
      }
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading asset details...</p>
        </div>
      </div>
    );
  }

  if (error || !asset) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 mb-4">{error || 'Asset not found'}</p>
          <button
            onClick={() => navigate('/dashboard')}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Back to Dashboard
          </button>
        </div>
      </div>
    );
  }

  const getStatusBadgeColor = (status) => {
    const colors = {
      'ACTIVE': 'bg-green-100 text-green-800',
      'INACTIVE': 'bg-gray-100 text-gray-800',
      'MAINTENANCE': 'bg-yellow-100 text-yellow-800',
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  const getTypeIcon = (type) => {
    const icons = {
      'CONTAINER': <FaBox className="inline mr-2" />,
      'VEHICLE': <FaTruck className="inline mr-2" />,
      'MACHINERY': <FaCog className="inline mr-2" />,
    };
    return icons[type] || <FaClipboard className="inline mr-2" />;
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-4xl mx-auto px-4 py-6 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between">
            <div>
              <button
                onClick={() => navigate('/dashboard')}
                className="text-blue-600 hover:text-blue-800 text-sm font-medium mb-4"
              >
                ← Back to Dashboard
              </button>
              <h1 className="text-3xl font-bold text-gray-900">
                {getTypeIcon(asset.type)} {asset.serialNumber}
              </h1>
            </div>
            <span className={`px-4 py-2 rounded-full text-sm font-semibold ${getStatusBadgeColor(asset.status)}`}>
              {asset.status}
            </span>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-4xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
        {/* Info Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-8">
          {/* Left Column */}
          <div>
            <h2 className="text-xl font-semibold text-gray-900 mb-4">Basic Information</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Type</label>
                <p className="mt-1 text-lg text-gray-900">{asset.type}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Serial Number</label>
                <p className="mt-1 text-lg font-mono text-gray-900">{asset.serialNumber}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Location</label>
                <p className="mt-1 text-lg text-gray-900">{asset.location}</p>
              </div>
              {asset.owner && (
                <div>
                  <label className="block text-sm font-medium text-gray-700">Owner</label>
                  <p className="mt-1 text-lg text-gray-900">{asset.owner}</p>
                </div>
              )}
            </div>
          </div>

          {/* Right Column */}
          <div>
            <h2 className="text-xl font-semibold text-gray-900 mb-4">Status & Metadata</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Created</label>
                <p className="mt-1 text-lg text-gray-900">
                  {new Date(asset.createdAt).toLocaleDateString()}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Last Updated</label>
                <p className="mt-1 text-lg text-gray-900">
                  {new Date(asset.updatedAt).toLocaleDateString()}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Health Score</label>
                {asset.healthScore !== null && asset.healthScore !== undefined ? (
                  <div className="mt-1 flex items-center">
                    <div className="flex-1 bg-gray-200 rounded-full h-2">
                      <div
                        className={`h-2 rounded-full ${
                          asset.healthScore >= 80
                            ? 'bg-green-600'
                            : asset.healthScore >= 60
                            ? 'bg-yellow-600'
                            : 'bg-red-600'
                        }`}
                        style={{ width: `${asset.healthScore}%` }}
                      ></div>
                    </div>
                    <span className="ml-2 text-lg font-semibold text-gray-900">
                      {asset.healthScore.toFixed(1)}%
                    </span>
                  </div>
                ) : (
                  <p className="mt-1 text-sm text-gray-500 italic">
                    No telemetry data available yet
                  </p>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Description */}
        {asset.description && (
          <div className="mb-8">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">Description</h2>
            <div className="bg-white rounded-lg p-6 border border-gray-200">
              <p className="text-gray-700 whitespace-pre-wrap">{asset.description}</p>
            </div>
          </div>
        )}

        {/* Maintenance Alerts */}
        <div className="mb-8">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Maintenance Status</h2>
          <MaintenanceAlerts 
            assetId={id} 
            apiBaseUrl={import.meta.env.VITE_API_BASE_URL}
          />
        </div>

        {/* Actions */}
        <div className="flex gap-4">
          <button
            onClick={() => navigate(`/dashboard`)}
            className="px-6 py-3 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition font-medium"
          >
            Edit Asset
          </button>
          <button
            onClick={handleDelete}
            className="px-6 py-3 bg-red-600 text-white rounded-lg hover:bg-red-700 transition font-medium"
          >
            Delete Asset
          </button>
        </div>
      </div>
    </div>
  );
}
