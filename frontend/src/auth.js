const STORAGE_KEY = "dotnet_test_auth";

export function getSession() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;

    const parsed = JSON.parse(raw);
    if (!parsed?.accessToken || !parsed?.expiresAtUtc) {
      return null;
    }

    return parsed;
  } catch {
    return null;
  }
}

export function saveSession(loginResponse) {
  const session = {
    accessToken: loginResponse.accessToken,
    expiresAtUtc: loginResponse.expiresAtUtc,
    userName: loginResponse.userName,
    role: loginResponse.role,
    playerId: loginResponse.playerId,
  };

  localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
}

export function clearSession() {
  localStorage.removeItem(STORAGE_KEY);
}

export function getToken() {
  return getSession()?.accessToken ?? null;
}

export function isAuthenticated() {
  const session = getSession();
  if (!session) return false;

  const expiresAt = Date.parse(session.expiresAtUtc);
  if (Number.isNaN(expiresAt)) return false;

  return expiresAt > Date.now();
}

export function isAdmin() {
  return getSession()?.role === "Admin";
}

export function canAccessPlayer(playerId) {
  if (isAdmin()) return true;

  const session = getSession();
  return session?.playerId === playerId;
}
