<script lang="ts">
	import ModeSwitcher from './ModeSwitcher.svelte';
	import Tailwindcss from './Tailwindcss.svelte';
	import QuoteList from './QuoteList.svelte';
	import NewQuote from './NewQuote.svelte';
</script>
<Tailwindcss />
<ModeSwitcher />
<main class="p-4 mx-auto max-w-3xl relative">
	<h1 class="uppercase text-4xl text-center leading-normal font-thin text-svelte mb-3 md:mb-6 md:text-6xl">Quoteboard</h1>
	<QuoteList />
	<NewQuote />
</main>