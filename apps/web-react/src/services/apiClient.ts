import type { ApiProblemDetails, DefaultLocationDto, WeatherViewModel } from '../types/weather';

const API_BASE = '/api';

async function parseJson<T>(response: Response): Promise<T> {
  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

function toErrorMessage(status: number, payload: unknown): string {
  const problem = payload as ApiProblemDetails | undefined;
  return problem?.detail || problem?.title || `Request failed with status ${status}.`;
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    headers: {
      'Content-Type': 'application/json'
    },
    ...init
  });

  if (!response.ok) {
    let payload: unknown;
    try {
      payload = await response.json();
    } catch {
      payload = undefined;
    }

    throw new Error(toErrorMessage(response.status, payload));
  }

  return parseJson<T>(response);
}

export function getWeather(city: string): Promise<WeatherViewModel> {
  const query = new URLSearchParams({ city });
  return request<WeatherViewModel>(`${API_BASE}/weather?${query.toString()}`);
}

export function getDefaultLocation(): Promise<DefaultLocationDto> {
  return request<DefaultLocationDto>(`${API_BASE}/settings/default-location`);
}

export function setDefaultLocation(city: string): Promise<void> {
  return request<void>(`${API_BASE}/settings/default-location`, {
    method: 'PUT',
    body: JSON.stringify({ city })
  });
}
