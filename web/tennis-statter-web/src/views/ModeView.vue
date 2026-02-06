<template>
  <div class="page">
    <h2 class="title">Singles or Doubles?</h2>

    <div class="row">
      <button class="mode-btn singles" :disabled="loading" @click="start('Singles')">
        Singles
      </button>
      <button class="mode-btn doubles" :disabled="loading" @click="start('Doubles')">
        Doubles
      </button>
    </div>

    <p v-if="loading">Creating match...</p>
    <p v-if="error" class="error">{{ error }}</p>
    <p v-if="store.matchId" class="ok">
      Created matchId: {{ store.matchId }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useMatchStore, type MatchMode } from "../stores/matchStore";

const store = useMatchStore();
const loading = ref(false);
const error = ref<string | null>(null);

// IMPORTANT: set this to your API base URL.
// Use the HTTPS URL your API prints in the console.
// Example: https://localhost:7251
const API_BASE = "https://localhost:7251";

async function start(mode: MatchMode) {
  loading.value = true;
  error.value = null;

  try {
    // Map string mode to backend enum values (Singles=0, Doubles=1)
    const modeValue = mode === "Singles" ? 0 : 1;

    const res = await fetch(`${API_BASE}/matches`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ mode: modeValue }),
    });

    if (!res.ok) throw new Error(`API error: ${res.status}`);

    const data = await res.json();
    store.setMatch(mode, data.matchId);

    // Later you’ll navigate to the singles/doubles tracking page here
    // router.push(`/match/${data.matchId}`);
  } catch (e: any) {
    error.value = e?.message ?? "Unknown error";
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  gap: 24px;
  background: #f7f7f7;
}
.title {
  font-size: 36px;
  font-weight: 500;
}
.row {
  display: flex;
  gap: 40px;
}
.mode-btn {
  width: 320px;
  height: 90px;
  border: 3px solid #111;
  font-size: 24px;
  cursor: pointer;
}
.singles { background: #f5d1ad; }
.doubles { background: #d9d9ff; }
.error { color: #b00020; }
.ok { color: #0a7a2f; }
</style>
