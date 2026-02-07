<template>
  <div class="page">
    <!-- Match Winner Popup -->
    <div v-if="matchWinner" class="popup-overlay">
      <div class="popup-box">
        <h2>Match Over!</h2>
        <p><strong>{{ matchWinner }}</strong> wins the match!</p>
        <button class="home-btn" @click="goHome">Back to Home</button>
      </div>
    </div>

    <div class="page-label">Singles Stat View</div>

    <!-- Score Box (top-left) -->
    <div class="score-box">
      <div class="score-row">
        <div class="player-cell">{{ topPlayer }}</div>
        <div class="score-cell">{{ inTiebreak ? topTiebreakPoints : pointScores[topPoint] }}</div>
        <div class="score-cell">{{ getSetScore('top', 0) }}</div>
        <div class="score-cell">{{ getSetScore('top', 1) }}</div>
        <div class="score-cell">{{ getSetScore('top', 2) }}</div>
      </div>
      <div class="score-row">
        <div class="player-cell">
          {{ bottomPlayer }}
          <span v-if="server === bottomPlayer" class="dot">●</span>
        </div>
        <div class="score-cell">{{ inTiebreak ? bottomTiebreakPoints : pointScores[bottomPoint] }}</div>
        <div class="score-cell">{{ getSetScore('bottom', 0) }}</div>
        <div class="score-cell">{{ getSetScore('bottom', 1) }}</div>
        <div class="score-cell">{{ getSetScore('bottom', 2) }}</div>
      </div>
    </div>

    <!-- Point Trail -->
    <div class="point-trail-box">
      <div class="point-trail-label">Point Trail</div>
    </div>

    <!-- Stat Entry Row 1 -->
    <div class="stat-row">
      <div class="stat-group">
        <div class="stat-label">1st Serve</div>
        <div class="stat-box">
          <div class="stat-col">
            <div>Wide</div>
            <div>Body</div>
            <div>Center</div>
            <div>In Net</div>
          </div>
          <div class="stat-col">
            <div>In Play</div>
            <div>Ace</div>
            <div>Winner</div>
            <div>Fault</div>
          </div>
        </div>
      </div>
      <div class="stat-group">
        <div class="stat-label">2nd Serve</div>
        <div class="stat-box">
          <div class="stat-col">
            <div>Wide</div>
            <div>Body</div>
            <div>Center</div>
            <div>In Net</div>
          </div>
          <div class="stat-col">
            <div>In Play</div>
            <div>Ace</div>
            <div>Winner</div>
            <div>Fault</div>
          </div>
        </div>
      </div>
      <div class="stat-group">
        <div class="stat-box stat-box-single">
          <div class="stat-col">
            <div>Forehand</div>
            <div>Backhand</div>
          </div>
        </div>
      </div>
      <div class="stat-group">
        <div class="stat-label">Return</div>
        <div class="stat-box">
          <div class="stat-col">
            <div>Ground</div>
            <div>Pass</div>
            <div>Approach</div>
            <div>Lob</div>
            <div>Drop</div>
          </div>
          <div class="stat-col">
            <div>In Play</div>
            <div>Winner</div>
            <div>Unforced</div>
            <div>Forced</div>
          </div>
        </div>
      </div>
      <div class="stat-group">
        <div class="stat-label">Point Winner</div>
        <div class="stat-box stat-box-single">
          <div class="stat-col">
            <div v-for="(option, idx) in pointWinnerOptions" :key="option">
              <span v-if="idx === selectedWinner">&gt; </span>
              <span :class="{ selected: idx === selectedWinner }">{{ option }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Stat Entry Row 2 -->
    <div class="stat-row">
      <div class="stat-group">
        <div class="stat-label">Point End</div>
        <div class="stat-box">
          <div class="stat-col">
            <div>Forehand</div>
            <div>Backhand</div>
          </div>
          <div class="stat-col">
            <div>Ground</div>
            <div>Pass</div>
            <div>Approach</div>
            <div>Lob</div>
            <div>Drop</div>
            <div>Overhead</div>
            <div>Volley</div>
          </div>
        </div>
      </div>
      <div class="stat-group">
        <div class="stat-label">Strategy</div>
        <div class="stat-box stat-box-single">
          <div class="stat-col">
            <div>Baseline</div>
            <div>Top Player At Net</div>
            <div>Bottom Player At Net</div>
            <div>Both At Net</div>
          </div>
        </div>
      </div>
      <div class="stat-group">
        <div class="stat-label">Serve &amp; Volley</div>
        <div class="stat-box stat-box-single">
          <div class="stat-col">
            <div>No Serve &amp; Volley</div>
            <div>Serve &amp; Volley</div>
          </div>
        </div>
      </div>
      <div class="stat-group">
        <div class="stat-box stat-box-summary">
          <div>Summary Stats So Far</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router';
import { ref, onMounted, onUnmounted } from 'vue';

const route = useRoute();
const router = useRouter();
const topPlayer = route.query.topPlayer as string || 'Top Player';
const bottomPlayer = route.query.bottomPlayer as string || 'Bottom Player';
const server = route.query.server as string || '';

const matchWinner = ref<string | null>(null);

const pointScores = ['0', '15', '30', '40', 'Ad'];
const topPoint = ref(0);
const bottomPoint = ref(0);
const topGames = ref(0);
const bottomGames = ref(0);

// Sets: array of [topGamesWon, bottomGamesWon] for each completed set
const topSets = ref<number[]>([]);
const bottomSets = ref<number[]>([]);
const currentSet = ref(1);
const inTiebreak = ref(false);
const topTiebreakPoints = ref(0);
const bottomTiebreakPoints = ref(0);

const pointWinnerOptions = ['Top Player', 'Bottom Player'];
const selectedWinner = ref(0);

// Helper to get the score for a set column
// Column 0 = first set, column 1 = second set, etc.
// Shows completed set score OR live games if it's the current set
function getSetScore(player: 'top' | 'bottom', colIndex: number): string | number {
  const sets = player === 'top' ? topSets.value : bottomSets.value;
  const games = player === 'top' ? topGames.value : bottomGames.value;
  
  // If this column has a completed set, show it
  if (colIndex < sets.length) {
    return sets[colIndex];
  }
  // If this is the current set column (immediately after completed sets), show live games
  if (colIndex === sets.length) {
    return games;
  }
  // Future sets - empty
  return '';
}

function handleKey(e: KeyboardEvent) {
  if (e.key === 'ArrowDown') {
    selectedWinner.value = (selectedWinner.value + 1) % pointWinnerOptions.length;
  } else if (e.key === 'ArrowUp') {
    selectedWinner.value = (selectedWinner.value - 1 + pointWinnerOptions.length) % pointWinnerOptions.length;
  } else if (e.key === 'Enter') {
    submitPointWinner();
  }
}

function submitPointWinner() {
  if (inTiebreak.value) {
    // Tiebreak scoring
    if (selectedWinner.value === 0) {
      topTiebreakPoints.value++;
    } else {
      bottomTiebreakPoints.value++;
    }
    checkTiebreakWin();
  } else {
    // Regular game scoring
    if (selectedWinner.value === 0) {
      updateScore(topPoint, bottomPoint, topGames, bottomGames);
    } else {
      updateScore(bottomPoint, topPoint, bottomGames, topGames);
    }
  }
}

function updateScore(
  myPoint: { value: number },
  oppPoint: { value: number },
  myGames: { value: number },
  oppGames: { value: number }
) {
  // Deuce: both at 40 (index 3)
  if (myPoint.value === 3 && oppPoint.value === 3) {
    // Go to advantage
    myPoint.value = 4;
  } else if (myPoint.value === 4) {
    // Win from advantage
    winGame(myGames, oppGames);
    myPoint.value = 0;
    oppPoint.value = 0;
  } else if (oppPoint.value === 4) {
    // Opponent had advantage, back to deuce
    oppPoint.value = 3;
  } else if (myPoint.value === 3 && oppPoint.value < 3) {
    // Win game from 40 when opponent doesn't have 40
    winGame(myGames, oppGames);
    myPoint.value = 0;
    oppPoint.value = 0;
  } else if (myPoint.value < 3) {
    myPoint.value++;
  }
}

function winGame(myGames: { value: number }, oppGames: { value: number }) {
  myGames.value++;
  checkSetWin();
}

function checkSetWin() {
  const tg = topGames.value;
  const bg = bottomGames.value;

  // Check for tiebreak at 6-6
  if (tg === 6 && bg === 6) {
    inTiebreak.value = true;
    topTiebreakPoints.value = 0;
    bottomTiebreakPoints.value = 0;
    return;
  }

  // Check if top player wins set
  if (tg >= 6 && tg - bg >= 2) {
    winSet('top');
    return;
  }

  // Check if bottom player wins set
  if (bg >= 6 && bg - tg >= 2) {
    winSet('bottom');
    return;
  }
}

function checkTiebreakWin() {
  const tp = topTiebreakPoints.value;
  const bp = bottomTiebreakPoints.value;

  // First to 7 with 2 point lead
  if (tp >= 7 && tp - bp >= 2) {
    topGames.value++; // Tiebreak counts as 1 game (7-6)
    winSet('top');
  } else if (bp >= 7 && bp - tp >= 2) {
    bottomGames.value++;
    winSet('bottom');
  }
}

function winSet(winner: 'top' | 'bottom') {
  // Store completed set scores
  topSets.value.push(topGames.value);
  bottomSets.value.push(bottomGames.value);

  // Check if match is won (best of 3 = first to 2 sets)
  const topSetsWon = topSets.value.filter((s, i) => s > bottomSets.value[i]).length;
  const bottomSetsWon = bottomSets.value.filter((s, i) => s > topSets.value[i]).length;

  if (topSetsWon >= 2) {
    matchWinner.value = topPlayer;
    return;
  }
  if (bottomSetsWon >= 2) {
    matchWinner.value = bottomPlayer;
    return;
  }

  // Reset for next set
  topGames.value = 0;
  bottomGames.value = 0;
  topPoint.value = 0;
  bottomPoint.value = 0;
  inTiebreak.value = false;
  topTiebreakPoints.value = 0;
  bottomTiebreakPoints.value = 0;
  currentSet.value++;
}

function goHome() {
  router.push('/');
}

onMounted(() => {
  window.addEventListener('keydown', handleKey);
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKey);
});
</script>

<style scoped>
.page {
  padding: 16px;
  font-family: sans-serif;
}
.page-label {
  font-size: 0.9rem;
  color: #888;
  margin-bottom: 8px;
}

/* Score Box */
.score-box {
  display: inline-block;
  border: 2px solid #111;
  margin-bottom: 24px;
}
.score-row {
  display: flex;
}
.player-cell {
  min-width: 120px;
  padding: 8px 12px;
  border-right: 2px solid #111;
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: 500;
}
.score-cell {
  width: 40px;
  min-width: 40px;
  padding: 8px 0;
  text-align: center;
  border-right: 2px solid #111;
  font-weight: 500;
}
.score-cell:last-child {
  border-right: none;
}
.score-row:first-child .player-cell,
.score-row:first-child .score-cell {
  border-bottom: 2px solid #111;
}
.dot {
  color: #8e267d;
  font-size: 1.2rem;
}

/* Point Trail */
.point-trail-box {
  border: 2px solid #111;
  min-height: 120px;
  padding: 16px;
  margin-bottom: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.point-trail-label {
  color: #aaa;
  font-size: 1rem;
}

/* Stat Rows */
.stat-row {
  display: flex;
  gap: 0;
  margin-bottom: 0;
}
.stat-group {
  display: flex;
  flex-direction: column;
}
.stat-label {
  font-size: 0.95rem;
  font-weight: 600;
  margin-bottom: 4px;
  padding-left: 4px;
}
.stat-box {
  display: flex;
  border: 2px solid #111;
  min-height: 80px;
}
.stat-box-single {
  min-width: 100px;
}
.stat-box-summary {
  min-width: 180px;
  min-height: 140px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #888;
}
.stat-col {
  padding: 8px 12px;
  border-right: 2px solid #111;
  display: flex;
  flex-direction: column;
  gap: 2px;
  font-size: 0.95rem;
}
.stat-col:last-child {
  border-right: none;
}
.selected {
  font-weight: bold;
  color: #8e267d;
}

/* Match Winner Popup */
.popup-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.popup-box {
  background: #fff;
  border: 3px solid #111;
  padding: 32px 48px;
  text-align: center;
  min-width: 300px;
}
.popup-box h2 {
  margin: 0 0 16px 0;
  font-size: 1.5rem;
}
.popup-box p {
  margin: 0 0 24px 0;
  font-size: 1.2rem;
}
.home-btn {
  background: #8e267d;
  color: #fff;
  border: none;
  padding: 12px 24px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
}
.home-btn:hover {
  background: #6d1d5f;
}
</style>
