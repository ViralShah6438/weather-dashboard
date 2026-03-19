import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { DefaultLocationComponent } from './default-location.component';

describe('DefaultLocationComponent', () => {
  let fixture: ComponentFixture<DefaultLocationComponent>;
  let component: DefaultLocationComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DefaultLocationComponent],
      imports: [FormsModule]
    }).compileComponents();

    fixture = TestBed.createComponent(DefaultLocationComponent);
    component = fixture.componentInstance;
    component.city = 'Madrid';
    component.ngOnChanges({
      city: {
        previousValue: '',
        currentValue: 'Madrid',
        firstChange: true,
        isFirstChange: () => true
      }
    });
    fixture.detectChanges();
  });

  it('emits save when city is valid', () => {
    spyOn(component.save, 'emit');
    component.draftCity = 'Rome';

    component.submit();

    expect(component.save.emit).toHaveBeenCalledWith('Rome');
  });

  it('emits trimmed city value when saving', () => {
    spyOn(component.save, 'emit');
    component.draftCity = '  Lisbon  ';

    component.submit();

    expect(component.save.emit).toHaveBeenCalledWith('Lisbon');
  });

  it('does not emit save when city is empty', () => {
    spyOn(component.save, 'emit');
    component.draftCity = ' ';

    component.submit();

    expect(component.save.emit).not.toHaveBeenCalled();
  });

  it('updates draftCity when city input changes', () => {
    component.city = 'Milan';

    component.ngOnChanges({
      city: {
        previousValue: 'Madrid',
        currentValue: 'Milan',
        firstChange: false,
        isFirstChange: () => false
      }
    });

    expect(component.draftCity).toBe('Milan');
  });

  it('does not overwrite draftCity when unrelated input changes', () => {
    component.draftCity = 'Custom Draft';

    component.ngOnChanges({
      disabled: {
        previousValue: false,
        currentValue: true,
        firstChange: false,
        isFirstChange: () => false
      }
    });

    expect(component.draftCity).toBe('Custom Draft');
  });

  it('does not alter draftCity when city input change value is unchanged', () => {
    component.draftCity = 'Barcelona';
    component.city = 'Barcelona';

    component.ngOnChanges({
      city: {
        previousValue: 'Barcelona',
        currentValue: 'Barcelona',
        firstChange: false,
        isFirstChange: () => false
      }
    });

    expect(component.draftCity).toBe('Barcelona');
  });
});
