import { Component, input } from '@angular/core';

@Component({
  selector: 'app-sample-hint',
  standalone: true,
  template: `<p class="sample-hint">{{ text() }}</p>`,
  styles: `
    .sample-hint {
      margin: 6px 0 14px;
      font-size: 11px;
      line-height: 1.3;
      color: #2563eb;
      font-style: italic;
    }
  `,
})
export class SampleHintComponent {
  readonly text = input.required<string>();
}
