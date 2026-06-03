// API client configuration module
import { clearSession, getToken } from "./auth";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5041";

const defaultHeaders = {
  "Content-Type": "application/json",
};

export async function apiCall(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  const token = getToken();
  const headers = {
    ...defaultHeaders,
    ...(options.headers || {}),
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  try {
    const response = await fetch(url, {
      headers,
      ...options,
    });

    if (!response.ok) {
      if (response.status === 401) {
        clearSession();
      }

      const body = await response.text();
      const details = body ? ` - ${body}` : "";
      throw new Error(`HTTP ${response.status}: ${response.statusText}${details}`);
    }

    if (response.status === 204) {
      return null;
    }

    return await response.json();
  } catch (error) {
    console.error(`API call failed for ${endpoint}:`, error);
    throw error;
  }
}

export function login(userName, password) {
  return apiCall("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ userName, password }),
  });
}

export function getPlayers() {
  return apiCall("/api/players");
}

export function getPlayer(playerId) {
  return apiCall(`/api/players/${playerId}`);
}

export function getRecommendations(playerId, maxResults = 3) {
  return apiCall(`/api/players/${playerId}/recommendations`, {
    method: "POST",
    body: JSON.stringify({ maxResults }),
  });
}
