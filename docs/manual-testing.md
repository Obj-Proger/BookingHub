# Manual testing

Scenarios to walk through by hand after `seed` (see the README). Run the API and the Web app;
in Development no email or SMS is sent — every message, with its link, appears in the **API
console** a few seconds after the action (the outbox processor polls every 5 seconds).

Staff accounts (password `Demo1234`): `owner@demo.test`, `admin@demo.test`,
`manager@demo.test` (Central only), `anna@demo.test` (employee Anna Smirnova).
Guest links mentioned below are printed by the seed command.

## Guest: booking

| # | Do | Expect |
|---|----|--------|
| 1 | Open `/book/demo` | Two locations: Central and Riverside |
| 2 | Riverside → Haircut | Price 1800 RUB (location override; Central shows 1500) |
| 3 | Pick a specialist and a day | Times in the location's own time zone (Moscow for Riverside, Yekaterinburg for Central) |
| 4 | Central → Anna, three working days ahead | No times: Anna has a day off |
| 5 | Book a free time with a new phone number | "Almost there"; the API console shows an SMS (and an email, if given) with a confirmation link |
| 6 | Open the confirmation link, press **Confirm booking** | "Booking confirmed"; the console shows a message with the manage link |
| 7 | Open the manage link, pick another time, **Confirm new time** | "Booking rescheduled"; a "moved to …" message in the console |
| 8 | On the manage page, **Cancel booking** | "Booking cancelled"; a cancellation message in the console |
| 9 | Book the same time in two tabs (two phone numbers), confirm both | The second confirmation fails: the time is no longer available |
| 10 | Make six bookings with one phone number within an hour | The sixth is refused: too many booking requests for this number |
| 11 | Open the pending-booking link from the seed output within 30 minutes and confirm | Confirmed. After 30 minutes the booking expires and the link no longer works |

## Guest: waitlist and reviews

| # | Do | Expect |
|---|----|--------|
| 12 | Open Ivan's manage link from the seed output and cancel | Within seconds the console shows "A slot opened up" for Pavel Orlov (+79001000005), with a link |
| 13 | Open Pavel's link, **Take this slot** | "Booked! See you then." The time is booked again |
| 14 | Pick a day with no free time (Anna's day off), **Join the waitlist** | "You're on the waitlist" |
| 15 | As owner, in **Bookings**, mark Oleg's past visit with Anna (yesterday, Beard trim) completed | A "How was your visit?" message with a review link in the console |
| 16 | Open the review link, give stars and a comment, submit | "Thanks for your feedback!"; the review appears in **Reviews** |

## Staff: roles and data

| # | Sign in as | Expect |
|---|-----------|--------|
| 17 | owner | Every location; Settings and Members are available; Analytics shows bookings and revenue; Reviews lists three seeded reviews, one hidden |
| 18 | owner → Reviews | Hide and unhide a review; a hidden review is not shown to guests |
| 19 | admin | Same as owner, but Analytics is refused until the owner allows administrators to see financials (Settings) |
| 20 | manager | Only Central: its bookings, schedule and analytics; opening Riverside is refused |
| 21 | anna | Only her own bookings and schedule |
| 22 | owner → Clients → search `+79001000001` | Ivan Petrov's profile with his visits |
| 23 | Register a new account (`POST /api/v1/auth/register` in the API's Scalar UI, `/scalar/v1` — the Web app has no sign-up page yet), sign in with it, create an organization | The new organization is empty: no demo data is visible from it |

## Background jobs

| # | Check | Expect |
|---|-------|--------|
| 24 | `https://localhost:7117/hangfire` (Development only) | Recurring jobs: expire pending bookings, awaiting review, auto-complete, expire waitlist offers, purge outbox |
| 25 | 30+ minutes after seeding | Elena's pending booking is Expired |
