import axios from "axios";

// Your ASP.NET Core API base URL
const API_BASE_URL = "https://localhost:7000/api"; // Adjust port if different

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add request interceptor for auth token (when you add authentication)
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("wagerwatch_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const wagerAPI = {
  // Wager endpoints (matching your controllers)
  getActiveWagers: () => api.get("/bets/active"),
  getSettledWagers: () => api.get("/bets/settled"),
  getAllWagers: () => api.get("/bets"),
  createWager: (wagerData) => api.post("/bets", wagerData),
  settleWager: (id, status) => api.put(`/bets/${id}/settle`, { status }),
  updateVolatility: (id, volatility) =>
    api.put(`/bets/${id}/volatility`, { volatility }),
  deleteWager: (id) => api.delete(`/bets/${id}`),
  getUserStats: () => api.get("/bets/stats"),

  // Game endpoints
  getUpcomingGames: () => api.get("/games/upcoming"),
  getLiveGames: () => api.get("/games/live"),
  updateGameScore: (id, scoreData) => api.put(`/games/${id}/score`, scoreData),

  // ESPN endpoints
  getNflGames: () => api.get("/espn/nfl"),
  getNbaGames: () => api.get("/espn/nba"),
  getMlbGames: () => api.get("/espn/mlb"),
  getNhlGames: () => api.get("/espn/nhl"),

  // Auth endpoints
  login: (credentials) => api.post("/auth/login", credentials),
  register: (userData) => api.post("/auth/register", userData),
  getCurrentUser: () => api.get("/auth/me"),
  getTimeZones: () => api.get("/auth/timezones"),
};

export default api;
