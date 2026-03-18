import { useCallback, useEffect, useState } from 'react';
import { getDefaultLocation, getWeather, setDefaultLocation } from '../services/apiClient';
import type { WeatherViewModel } from '../types/weather';

export function useWeatherDashboard() {
  const [weather, setWeather] = useState<WeatherViewModel | null>(null);
  const [defaultLocation, setDefaultLocationState] = useState<string>('London');
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  const search = useCallback(async (city: string) => {
    const value = city.trim();
    if (!value) {
      return;
    }

    setIsLoading(true);
    setErrorMessage(null);
    setStatusMessage(null);
    try {
      const snapshot = await getWeather(value);
      setWeather(snapshot);
    } catch (error) {
      setWeather(null);
      setErrorMessage(error instanceof Error ? error.message : 'Unable to fetch weather data.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  const saveDefaultLocation = useCallback(async (city: string) => {
    const value = city.trim();
    if (!value) {
      return;
    }

    setIsLoading(true);
    setErrorMessage(null);
    setStatusMessage(null);
    try {
      await setDefaultLocation(value);
      setDefaultLocationState(value);
      setStatusMessage(`Default location updated to ${value}.`);
      const snapshot = await getWeather(value);
      setWeather(snapshot);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to update default location.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    let active = true;

    async function loadInitialState() {
      setIsLoading(true);
      try {
        const setting = await getDefaultLocation();
        if (!active) {
          return;
        }

        const city = setting.city?.trim() || 'London';
        setDefaultLocationState(city);
        const snapshot = await getWeather(city);
        if (!active) {
          return;
        }

        setWeather(snapshot);
      } catch (error) {
        if (active) {
          setErrorMessage(error instanceof Error ? error.message : 'Unable to load default location.');
        }
      } finally {
        if (active) {
          setIsLoading(false);
        }
      }
    }

    loadInitialState();
    return () => {
      active = false;
    };
  }, []);

  return {
    weather,
    defaultLocation,
    isLoading,
    errorMessage,
    statusMessage,
    search,
    saveDefaultLocation
  };
}
