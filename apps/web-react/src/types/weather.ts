export interface WeatherViewModel {
  city: string;
  temperatureC: number;
  humidity: number;
  windSpeedKph: number;
  iconCode: string;
  description: string;
}

export interface DefaultLocationDto {
  city: string;
}

export interface ApiProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
}
