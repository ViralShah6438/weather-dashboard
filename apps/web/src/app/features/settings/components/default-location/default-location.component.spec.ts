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

  it('does not emit save when city is empty', () => {
    spyOn(component.save, 'emit');
    component.draftCity = ' ';

    component.submit();

    expect(component.save.emit).not.toHaveBeenCalled();
  });
});
