import { FormEvent, useEffect, useState } from 'react';

type DefaultLocationFormProps = {
  city: string;
  disabled?: boolean;
  onSave: (city: string) => void;
};

export function DefaultLocationForm({ city, disabled, onSave }: DefaultLocationFormProps) {
  const [draftCity, setDraftCity] = useState(city);

  useEffect(() => {
    setDraftCity(city);
  }, [city]);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const value = draftCity.trim();
    if (!value) {
      return;
    }

    onSave(value);
  }

  return (
    <form className="default-location" onSubmit={handleSubmit}>
      <label htmlFor="default-location-city">Default location</label>
      <div className="row">
        <input
          id="default-location-city"
          value={draftCity}
          onChange={(event) => setDraftCity(event.target.value)}
          placeholder="Set default city"
          disabled={disabled}
        />
        <button type="submit" disabled={disabled}>
          Save
        </button>
      </div>
    </form>
  );
}
