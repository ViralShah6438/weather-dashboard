import { FormEvent, useState } from 'react';

type SearchBarProps = {
  disabled?: boolean;
  onSearch: (city: string) => void;
};

export function SearchBar({ disabled, onSearch }: SearchBarProps) {
  const [city, setCity] = useState('');

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const value = city.trim();
    if (!value) {
      return;
    }

    onSearch(value);
  }

  return (
    <form className="search" onSubmit={handleSubmit}>
      <input
        type="text"
        placeholder="Search city"
        value={city}
        onChange={(event) => setCity(event.target.value)}
        disabled={disabled}
      />
      <button type="submit" disabled={disabled}>
        Search
      </button>
    </form>
  );
}
