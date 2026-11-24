import { renderMarkup } from 'shared';

describe('renderMarkup', () => {
  it('renders emphasis and line breaks', () => {
    expect(renderMarkup('From first line<br>to *last bolt*')).toBe('From first line<br>to <em>last bolt</em>');
  });
  it('escapes everything else', () => {
    expect(renderMarkup('<script>alert(1)</script> & *x*')).toBe('&lt;script&gt;alert(1)&lt;/script&gt; &amp; <em>x</em>');
  });
});
