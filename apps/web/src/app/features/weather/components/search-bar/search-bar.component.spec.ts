import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { SearchBarComponent } from './search-bar.component';

describe('SearchBarComponent', () => {
  let fixture: ComponentFixture<SearchBarComponent>;
  let component: SearchBarComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SearchBarComponent],
      imports: [FormsModule]
    }).compileComponents();

    fixture = TestBed.createComponent(SearchBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('emits search value on submit', () => {
    spyOn(component.search, 'emit');
    component.city = 'Berlin';

    component.submit();

    expect(component.search.emit).toHaveBeenCalledWith('Berlin');
  });

  it('emits trimmed search value on submit', () => {
    spyOn(component.search, 'emit');
    component.city = '  Tokyo  ';

    component.submit();

    expect(component.search.emit).toHaveBeenCalledWith('Tokyo');
  });

  it('does not emit for empty value', () => {
    spyOn(component.search, 'emit');
    component.city = '   ';

    component.submit();

    expect(component.search.emit).not.toHaveBeenCalled();
  });

  it('uses default placeholder text', () => {
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input[name="city"]');

    expect(component.placeholder).toBe('Search city');
    expect(input.placeholder).toBe('Search city');
  });

  it('uses custom placeholder when provided', () => {
    component.placeholder = 'Type a city';
    fixture.detectChanges();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input[name="city"]');

    expect(input.placeholder).toBe('Type a city');
  });
});
