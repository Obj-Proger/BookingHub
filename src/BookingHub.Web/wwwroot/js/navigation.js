// Brings the link to the current page into view inside a navigation row that scrolls sideways.
// 'nearest' leaves an already visible link, and the page's own vertical scroll, alone.
export function revealActiveLink(containerSelector) {
    document.querySelector(`${containerSelector} a.active`)
        ?.scrollIntoView({ block: 'nearest', inline: 'nearest' });
}
