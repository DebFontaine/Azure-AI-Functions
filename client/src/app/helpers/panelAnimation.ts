import { trigger, state, style, animate, transition } from '@angular/animations';

export const panelAnimation = trigger('panelAnimation', [
  state('closed', style({
    width: '0%',
    display: 'none'
  })),
  state('opened', style({
    width: '100%'
  })),
  transition('closed => opened', [
    style({ width: '0%' }), // Initial style for opening
    animate('400ms ease-in-out', style({ width: '100%' })) // Final style for opening
  ]),
  transition('opened => closed', [
    style({ width: '100%' }), // Initial style for closing
    animate('300ms linear', style({ width: '0%' })) // Final style for closing
  ])
]);
