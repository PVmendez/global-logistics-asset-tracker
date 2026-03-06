import { create } from 'zustand';

export const useAuthStore = create((set) => ({
  token: localStorage.getItem('authToken') || null,
  user: null,
  isLoading: false,
  error: null,

  setToken: (token) => {
    if (token) {
      localStorage.setItem('authToken', token);
    } else {
      localStorage.removeItem('authToken');
    }
    set({ token });
  },

  setUser: (user) => set({ user }),
  setLoading: (isLoading) => set({ isLoading }),
  setError: (error) => set({ error }),
  logout: () => {
    localStorage.removeItem('authToken');
    set({ token: null, user: null });
  },
}));

export const useAssetStore = create((set) => ({
  assets: [],
  currentAsset: null,
  pagination: { page: 1, pageSize: 20, total: 0, totalPages: 0 },
  isLoading: false,
  error: null,

  setAssets: (assets) => set({ assets }),
  setCurrentAsset: (currentAsset) => set({ currentAsset }),
  setPagination: (pagination) => set({ pagination }),
  setLoading: (isLoading) => set({ isLoading }),
  setError: (error) => set({ error }),

  addAsset: (asset) =>
    set((state) => ({
      assets: [asset, ...state.assets],
      pagination: { ...state.pagination, total: state.pagination.total + 1 },
    })),

  updateAsset: (assetId, updatedAsset) =>
    set((state) => ({
      assets: state.assets.map((a) => (a.id === assetId ? updatedAsset : a)),
      currentAsset: state.currentAsset?.id === assetId ? updatedAsset : state.currentAsset,
    })),

  removeAsset: (assetId) =>
    set((state) => ({
      assets: state.assets.filter((a) => a.id !== assetId),
      pagination: { ...state.pagination, total: state.pagination.total - 1 },
    })),
}));

export const useHealthStore = create((set) => ({
  assetHealth: {},
  systemHealth: null,
  isLoading: false,
  error: null,

  setAssetHealth: (assetId, health) =>
    set((state) => ({
      assetHealth: { ...state.assetHealth, [assetId]: health },
    })),

  setSystemHealth: (systemHealth) => set({ systemHealth }),
  setLoading: (isLoading) => set({ isLoading }),
  setError: (error) => set({ error }),
}));
