<script lang="ts">
  import { onMount } from "svelte";
  import Quote from "./Quote.svelte";
  import { loadQuotes, quotes } from "./data";

  onMount(() => {
    loadQuotes();
  });
</script>
<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
  {#each $quotes as quote (quote.id)}
    <Quote {quote} />
  {/each}
</div>