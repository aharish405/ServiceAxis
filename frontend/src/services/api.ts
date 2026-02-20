import axios from 'axios';

// Create a base axios instance configured for the ServiceAxis API
export const api = axios.create({
  baseURL: '/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to attach the JWT token if it exists
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});

// Add a response interceptor for handling common errors and unwrapping data
api.interceptors.response.use(
  (response) => {
    // If the response follows our ApiResponse envelope, return the payload
    if (response.data && Object.prototype.hasOwnProperty.call(response.data, 'success')) {
      return response.data.data;
    }
    return response.data;
  },
  (error) => {
    if (error.response?.status === 401) {
      console.error('Session expired. Please log in again.');
      // window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
