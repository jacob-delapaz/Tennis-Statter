<template>
  <div class="page">
    <h1 class="title">Who's serving?</h1>
    <div v-if="!serverPicked">
      <div v-if="isSingles">
        <button class="btn" @click="pickServer(topPlayer)">{{ topPlayer }}</button>
        <button class="btn" @click="pickServer(bottomPlayer)">{{ bottomPlayer }}</button>
      </div>
      <div v-else>
        <button class="btn" @click="pickServer(topPlayer1)">{{ topPlayer1 }}</button>
        <button class="btn" @click="pickServer(topPlayer2)">{{ topPlayer2 }}</button>
        <button class="btn" @click="pickServer(bottomPlayer1)">{{ bottomPlayer1 }}</button>
        <button class="btn" @click="pickServer(bottomPlayer2)">{{ bottomPlayer2 }}</button>
      </div>
    </div>
    <div v-else-if="isDoubles && !receiverPicked">
      <h2 class="subtitle">Who's receiving?</h2>
      <button class="btn" v-for="player in receiverOptions" :key="player" @click="pickReceiver(player)">{{ player }}</button>
    </div>
    <div v-else>
      <p>Server: {{ server }}</p>
      <p>Receiver: {{ receiver }}</p>
      <!-- Next step button here -->
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { useRoute, useRouter } from "vue-router";

const route = useRoute();
const router = useRouter();
const matchType = route.params.matchType as string;
const playersParam = route.query.players as string;
let players: string[] = [];
try {
  players = playersParam ? JSON.parse(playersParam) : [];
} catch {
  players = [];
}
console.log('ServeSelectionView players:', players);

const isSingles = matchType === "singles";
const isDoubles = matchType === "doubles";

const topPlayer = players[0] ?? "Unknown";
const bottomPlayer = players[1] ?? "Unknown";
const topPlayer1 = players[0] ?? "Unknown";
const topPlayer2 = players[1] ?? "Unknown";
const bottomPlayer1 = players[2] ?? "Unknown";
const bottomPlayer2 = players[3] ?? "Unknown";

const server = ref<string | null>(null);
const receiver = ref<string | null>(null);
const serverPicked = computed(() => !!server.value);
const receiverPicked = computed(() => !!receiver.value);

function pickServer(player: string) {
  server.value = player;
  if (isSingles) {
    receiver.value = player === topPlayer ? bottomPlayer : topPlayer;
  }
}

// Watch for both server and receiver being set in singles, then navigate
watch(
  () => [server.value, receiver.value],
  ([s, r]) => {
    if (isSingles && s && r) {
      router.push({
        name: "SinglesStat",
        query: {
          topPlayer,
          bottomPlayer,
          server: s,
        },
      });
    }
  }
);

const receiverOptions = computed(() => {
  if (!server.value) return [];
  // In doubles, receiver must be from opposing team
  if ([topPlayer1, topPlayer2].includes(server.value)) {
    return [bottomPlayer1, bottomPlayer2].filter((p): p is string => !!p);
  } else {
    return [topPlayer1, topPlayer2].filter((p): p is string => !!p);
  }
});

function pickReceiver(player: string) {
  receiver.value = player;
}
</script>

<style scoped>
.page {
  max-width: 640px;
  margin: 0 auto;
  padding: 48px 20px;
  display: flex;
  flex-direction: column;
  gap: 22px;
  background: white;
}
.title {
  text-align: center;
  font-size: 32px;
}
.subtitle {
  text-align: center;
  font-size: 24px;
  margin-bottom: 16px;
}
.btn {
  width: 320px;
  height: 90px;
  border: 3px solid #111;
  font-size: 24px;
  cursor: pointer;
  margin-bottom: 16px;
}
</style>
