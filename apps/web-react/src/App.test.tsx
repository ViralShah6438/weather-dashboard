import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from './App';

vi.mock('./services/apiClient', () => ({
  getDefaultLocation: vi.fn().mockResolvedValue({ city: 'London' }),
  getWeather: vi.fn().mockResolvedValue({
    city: 'London',
    temperatureC: 20,
    humidity: 50,
    windSpeedKph: 12,
    iconCode: '01d',
    description: 'clear sky'
  }),
  setDefaultLocation: vi.fn().mockResolvedValue(undefined)
}));

describe('App', () => {
  it('loads default location weather and renders weather card', async () => {
    render(<App />);

    await waitFor(() => {
      expect(screen.getByText(/London/i)).toBeInTheDocument();
    });

    expect(screen.getByText(/Temperature/i)).toBeInTheDocument();
  });

  it('submits city search', async () => {
    const user = userEvent.setup();
    render(<App />);

    const input = screen.getByPlaceholderText(/search city/i);
    await user.type(input, 'Paris');
    await user.click(screen.getByRole('button', { name: /search/i }));

    await waitFor(() => {
      expect(screen.getByText(/Temperature/i)).toBeInTheDocument();
    });
  });
});
