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

    <!-- Point Confirmation Popup -->
    <div v-if="showConfirmation" class="popup-overlay">
      <div class="popup-box">
        <p v-if="editMode">Update point: <strong>{{ confirmationWinnerName }}</strong> wins?</p>
        <p v-else>Confirming that <strong>{{ confirmationWinnerName }}</strong> are the point winners?</p>
        <div class="popup-buttons">
          <button class="confirm-btn yes-btn" @click="confirmPoint">Yes</button>
          <button class="confirm-btn no-btn" @click="cancelConfirmation">No</button>
        </div>
      </div>
    </div>

    <!-- Point Winner Error Popup -->
    <div v-if="showPointWinnerError" class="popup-overlay">
      <div class="popup-box">
        <p>Point Winner Not Selected</p>
        <div class="popup-buttons">
          <button class="confirm-btn yes-btn" @click="dismissPointWinnerError">OK</button>
        </div>
      </div>
    </div>

    <!-- Edit Mode Banner -->
    <div v-if="editMode" class="edit-mode-banner">
      <span>✏️ Editing Point {{ pointHistory[editingPointIndex!]?.setNumber }}-{{ pointHistory[editingPointIndex!]?.gameNumber }}-{{ pointHistory[editingPointIndex!]?.pointNumber }}</span>
      <button class="cancel-edit-btn" @click="exitEditMode">Cancel</button>
    </div>

    <!-- Radar Activation Box (top-right) -->
    <div class="top-row">
      <div class="score-box">
        <div class="score-row">
          <div class="player-cell">
            <span v-if="team1Players.includes(server)" class="dot">●</span>
            {{ topTeam }}
          </div>
          <div class="score-cell">{{ inTiebreak ? topTiebreakPoints : pointScores[topPoint] }}</div>
          <div class="score-cell">{{ getSetScore('top', 0) }}</div>
          <div class="score-cell">{{ getSetScore('top', 1) }}</div>
          <div class="score-cell">{{ getSetScore('top', 2) }}</div>
        </div>
        <div class="score-row">
          <div class="player-cell">
            {{ bottomTeam }}
            <span v-if="team2Players.includes(server)" class="dot">●</span>
          </div>
          <div class="score-cell">{{ inTiebreak ? bottomTiebreakPoints : pointScores[bottomPoint] }}</div>
          <div class="score-cell">{{ getSetScore('bottom', 0) }}</div>
          <div class="score-cell">{{ getSetScore('bottom', 1) }}</div>
          <div class="score-cell">{{ getSetScore('bottom', 2) }}</div>
        </div>
      </div>
      <div class="radar-box" :class="{ 'radar-active': radarActivated }">
        {{ radarActivated ? 'Radar Activated' : 'Activate Radar' }}
      </div>
    </div>

    <!-- Point Trail -->
    <div class="point-trail-box" ref="pointTrailBox">
      <table class="point-trail-table">
        <thead>
          <tr>
            <th>Point</th>
            <th>Winner</th>
            <th>1st Serve</th>
            <th>2nd Serve</th>
            <th>Return</th>
            <th>Point End</th>
            <th>Player</th>
            <th>S&V</th>
          </tr>
        </thead>
        <tbody>
          <!-- Empty placeholder rows at top to ensure minimum 3 rows -->
          <tr v-for="n in Math.max(0, 3 - pointHistory.length)" :key="'empty-' + n" class="empty-row">
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
          </tr>
          <tr v-for="(point, index) in pointHistory" :key="index" 
              class="point-trail-row"
              :class="{ 'editing-row': editMode && editingPointIndex === index }"
              @click="enterEditMode(index)">
            <td>{{ point.setNumber }}-{{ point.gameNumber }}-{{ point.pointNumber }}</td>
            <td>{{ point.pointWinner }}</td>
            <td>{{ point.firstServe }}</td>
            <td>{{ point.secondServe }}</td>
            <td>{{ point.returnStats }}</td>
            <td>{{ point.pointEnd }}</td>
            <td>{{ point.player }}</td>
            <td>{{ point.serveVolley }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Stat Entry Row 1 -->
    <div class="stat-row">
      <!-- 1st Serve: 2 boxes -->
      <div class="stat-group">
        <div class="stat-label-center">1st Serve</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'firstServeLocation' }" @click="activeCategory = 'firstServeLocation'">
            <div class="stat-col">
              <div v-for="(opt, idx) in serveLocationOptions" :key="opt" 
                   class="stat-option"
                   :class="{ 'selected-row': selections.firstServeLocation === idx }"
                   @click.stop="selections.firstServeLocation = idx; activeCategory = 'firstServeLocation'">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'firstServeResult' }" @click="activeCategory = 'firstServeResult'">
            <div class="stat-col">
              <div v-for="(opt, idx) in serveResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.firstServeResult === idx }"
                   @click.stop="selections.firstServeResult = idx; activeCategory = 'firstServeResult'">
                {{ opt }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- 2nd Serve: 2 boxes -->
      <div class="stat-group" :class="{ 'disabled-group': secondServeDisabled }">
        <div class="stat-label-center">2nd Serve</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'secondServeLocation' && !secondServeDisabled, 'disabled-box': secondServeDisabled }" @click="!secondServeDisabled && (activeCategory = 'secondServeLocation')">
            <div class="stat-col">
              <div v-for="(opt, idx) in serveLocationOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.secondServeLocation === idx && !secondServeDisabled }"
                   @click.stop="!secondServeDisabled && (selections.secondServeLocation = idx, activeCategory = 'secondServeLocation')">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'secondServeResult' && !secondServeDisabled, 'disabled-box': secondServeDisabled }" @click="!secondServeDisabled && (activeCategory = 'secondServeResult')">
            <div class="stat-col">
              <div v-for="(opt, idx) in serveResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.secondServeResult === idx && !secondServeDisabled }"
                   @click.stop="!secondServeDisabled && (selections.secondServeResult = idx, activeCategory = 'secondServeResult')">
                {{ opt }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- Return: 3 boxes -->
      <div class="stat-group" :class="{ 'disabled-group': returnDisabled }">
        <div class="stat-label-center">Return</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'returnSide' && !returnDisabled, 'disabled-box': returnDisabled }" @click="!returnDisabled && (activeCategory = 'returnSide')">
            <div class="stat-col">
              <div v-for="(opt, idx) in returnSideOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.returnSide === idx && !returnDisabled }"
                   @click.stop="!returnDisabled && (selections.returnSide = idx, activeCategory = 'returnSide')">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'returnType' && !returnDisabled, 'disabled-box': returnDisabled }" @click="!returnDisabled && (activeCategory = 'returnType')">
            <div class="stat-col">
              <div v-for="(opt, idx) in returnTypeOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.returnType === idx && !returnDisabled }"
                   @click.stop="!returnDisabled && (selections.returnType = idx, activeCategory = 'returnType')">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'returnResult' && !returnDisabled, 'disabled-box': returnDisabled }" @click="!returnDisabled && (activeCategory = 'returnResult')">
            <div class="stat-col">
              <div v-for="(opt, idx) in returnResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.returnResult === idx && !returnDisabled }"
                   @click.stop="!returnDisabled && (selections.returnResult = idx, activeCategory = 'returnResult')">
                {{ opt }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- Point Winner: 1 box -->
      <div class="stat-group">
        <div class="stat-label-center">Point Winner</div>
        <div class="stat-box point-winner-box selectable-box" :class="{ 'active-box': activeCategory === 'pointWinner' }" @click="activeCategory = 'pointWinner'">
          <div v-for="(option, idx) in pointWinnerOptions" :key="option" 
               class="point-winner-option"
               :class="{ 'selected-row': idx === selections.pointWinner }"
               @click.stop="selections.pointWinner = idx; activeCategory = 'pointWinner'">
            {{ option }}
          </div>
        </div>
      </div>
    </div>

    <!-- Stat Entry Row 2 -->
    <div class="stat-row">
      <!-- Point End: 2 boxes -->
      <div class="stat-group" :class="{ 'disabled-group': pointEndDisabled }">
        <div class="stat-label-center">Point End</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'pointEndSide' && !pointEndDisabled, 'disabled-box': pointEndDisabled }" @click="!pointEndDisabled && (activeCategory = 'pointEndSide')">
            <div class="stat-col">
              <div v-for="(opt, idx) in pointEndSideOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.pointEndSide === idx && !pointEndDisabled }"
                   @click.stop="!pointEndDisabled && (selections.pointEndSide = idx, activeCategory = 'pointEndSide')">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'pointEndType' && !pointEndDisabled, 'disabled-box': pointEndDisabled }" @click="!pointEndDisabled && (activeCategory = 'pointEndType')">
            <div class="stat-col">
              <div v-for="(opt, idx) in pointEndTypeOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.pointEndType === idx && !pointEndDisabled }"
                   @click.stop="!pointEndDisabled && (selections.pointEndType = idx, activeCategory = 'pointEndType')">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'pointEndResult' && !pointEndDisabled, 'disabled-box': pointEndDisabled }" @click="!pointEndDisabled && (activeCategory = 'pointEndResult')">
            <div class="stat-col">
              <div v-for="(opt, idx) in pointEndResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.pointEndResult === idx && !pointEndDisabled }"
                   @click.stop="!pointEndDisabled && (selections.pointEndResult = idx, activeCategory = 'pointEndResult')">
                {{ opt }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- Player: 1 box -->
      <div class="stat-group stat-group-strategy" :class="{ 'disabled-group': pointEndDisabled }">
        <div class="stat-label-center">Player</div>
        <div class="stat-box stat-box-strategy selectable-box" :class="{ 'active-box': activeCategory === 'player' && !pointEndDisabled, 'disabled-box': pointEndDisabled }" @click="!pointEndDisabled && (activeCategory = 'player')">
          <div class="stat-col">
            <div v-for="(opt, idx) in playerOptions" :key="opt"
                 class="stat-option"
                 :class="{ 'selected-row': selections.player === idx && !pointEndDisabled }"
                 @click.stop="!pointEndDisabled && (selections.player = idx, activeCategory = 'player')">
              {{ opt }}
            </div>
          </div>
        </div>
      </div>
      <!-- Serve & Volley: 1 box -->
      <div class="stat-group stat-group-expand" :class="{ 'disabled-group': serveVolleyDisabled }">
        <div class="stat-label-center">S&amp;V</div>
        <div class="stat-box stat-box-sv selectable-box" :class="{ 'active-box': activeCategory === 'serveVolley' && !serveVolleyDisabled, 'disabled-box': serveVolleyDisabled }" @click="!serveVolleyDisabled && (activeCategory = 'serveVolley')">
          <div class="stat-col">
            <div v-for="(opt, idx) in serveVolleyOptions" :key="idx"
                 class="stat-option"
                 :class="{ 'selected-row': selections.serveVolley === idx && !serveVolleyDisabled }"
                 @click.stop="!serveVolleyDisabled && (selections.serveVolley = idx, activeCategory = 'serveVolley')">
              {{ opt || '\u00A0' }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Summary Statistics Row -->
    <div class="stat-row summary-row-container">
      <div class="stat-group summary-stats-group">
        <div class="stat-box stat-box-summary">
          <div class="summary-stats-container">
            <!-- Team names header row -->
            <div class="summary-header-row">
              <div class="summary-player-header">{{ topTeam }}</div>
              <div class="summary-center-spacer"></div>
              <div class="summary-player-header">{{ bottomTeam }}</div>
            </div>
            <!-- Match/Set labels row -->
            <div class="summary-subheader-row">
              <div class="summary-col-label">Match</div>
              <div class="summary-col-label">Set</div>
              <div class="summary-center-spacer"></div>
              <div class="summary-col-label">Set</div>
              <div class="summary-col-label">Match</div>
            </div>
            <!-- 1st Serve % -->
            <div class="summary-data-row">
              <div class="summary-val">{{ topTeamStats.firstServePercentage }}%</div>
              <div class="summary-val">{{ topTeamStats.firstServePercentage }}%</div>
              <div class="summary-stat-label">1st Serve %</div>
              <div class="summary-val">{{ bottomTeamStats.firstServePercentage }}%</div>
              <div class="summary-val">{{ bottomTeamStats.firstServePercentage }}%</div>
            </div>
            <!-- 1st Serve Points Won % -->
            <div class="summary-data-row">
              <div class="summary-val">{{ topTeamStats.firstServePointsWonPercentage }}%</div>
              <div class="summary-val">{{ topTeamStats.firstServePointsWonPercentage }}%</div>
              <div class="summary-stat-label">1st Serve Pts Won %</div>
              <div class="summary-val">{{ bottomTeamStats.firstServePointsWonPercentage }}%</div>
              <div class="summary-val">{{ bottomTeamStats.firstServePointsWonPercentage }}%</div>
            </div>
            <!-- Aces -->
            <div class="summary-data-row">
              <div class="summary-val">{{ topTeamStats.aces }}</div>
              <div class="summary-val">{{ topTeamStats.aces }}</div>
              <div class="summary-stat-label">Aces</div>
              <div class="summary-val">{{ bottomTeamStats.aces }}</div>
              <div class="summary-val">{{ bottomTeamStats.aces }}</div>
            </div>
            <!-- Double Faults -->
            <div class="summary-data-row">
              <div class="summary-val">{{ topTeamStats.doubleFaults }}</div>
              <div class="summary-val">{{ topTeamStats.doubleFaults }}</div>
              <div class="summary-stat-label">Double Faults</div>
              <div class="summary-val">{{ bottomTeamStats.doubleFaults }}</div>
              <div class="summary-val">{{ bottomTeamStats.doubleFaults }}</div>
            </div>
            <!-- Forehand Winners -->
            <div class="summary-data-row">
              <div class="summary-val">{{ topTeamStats.forehandWinners }}</div>
              <div class="summary-val">{{ topTeamStats.forehandWinners }}</div>
              <div class="summary-stat-label">Forehand Winners</div>
              <div class="summary-val">{{ bottomTeamStats.forehandWinners }}</div>
              <div class="summary-val">{{ bottomTeamStats.forehandWinners }}</div>
            </div>
            <!-- Backhand Winners -->
            <div class="summary-data-row">
              <div class="summary-val">{{ topTeamStats.backhandWinners }}</div>
              <div class="summary-val">{{ topTeamStats.backhandWinners }}</div>
              <div class="summary-stat-label">Backhand Winners</div>
              <div class="summary-val">{{ bottomTeamStats.backhandWinners }}</div>
              <div class="summary-val">{{ bottomTeamStats.backhandWinners }}</div>
            </div>
            <!-- Net Points Won -->
            <div class="summary-data-row">
              <div class="summary-val">{{ topTeamStats.netPointsWon }}/{{ topTeamStats.netPointsTotal }}</div>
              <div class="summary-val">{{ topTeamStats.netPointsWon }}/{{ topTeamStats.netPointsTotal }}</div>
              <div class="summary-stat-label">Net Points Won</div>
              <div class="summary-val">{{ bottomTeamStats.netPointsWon }}/{{ bottomTeamStats.netPointsTotal }}</div>
              <div class="summary-val">{{ bottomTeamStats.netPointsWon }}/{{ bottomTeamStats.netPointsTotal }}</div>
            </div>
            <!-- Unforced Errors -->
            <div class="summary-data-row">
              <div class="summary-val">{{ topTeamStats.unforcedErrors }}</div>
              <div class="summary-val">{{ topTeamStats.unforcedErrors }}</div>
              <div class="summary-stat-label">Unforced Errors</div>
              <div class="summary-val">{{ bottomTeamStats.unforcedErrors }}</div>
              <div class="summary-val">{{ bottomTeamStats.unforcedErrors }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router';
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue';

const route = useRoute();
const router = useRouter();

// Team 1 (top) players
const team1Player1 = route.query.team1Player1 as string || 'Player 1';
const team1Player2 = route.query.team1Player2 as string || 'Player 2';
// Team 2 (bottom) players
const team2Player1 = route.query.team2Player1 as string || 'Player 3';
const team2Player2 = route.query.team2Player2 as string || 'Player 4';

// Team display names
const topTeam = `${team1Player1}/${team1Player2}`;
const bottomTeam = `${team2Player1}/${team2Player2}`;

// All players for reference
const team1Players = [team1Player1, team1Player2];
const team2Players = [team2Player1, team2Player2];

const initialServer = route.query.server as string || '';
const initialReceiver = route.query.receiver as string || '';

// Current server (alternates each game in doubles rotation)
const server = ref(initialServer);

// Doubles serve rotation: team1 player, team2 player, team1 other player, team2 other player
// Track the serve order for proper rotation
const serveOrder = ref<string[]>([]);
const currentServeOrderIndex = ref(0);

// Initialize serve order based on initial server
function initializeServeOrder() {
  const firstServer = initialServer;
  let firstTeam1Server: string;
  let firstTeam2Server: string;
  
  if (team1Players.includes(firstServer)) {
    // Team 1 serves first
    firstTeam1Server = firstServer;
    // Team 2's first server is set when they receive first (we'll use the receiver)
    firstTeam2Server = initialReceiver || team2Player1;
    // Order: T1-P1, T2-P1, T1-P2, T2-P2 (or T2's other player)
    const otherTeam1 = team1Players.find(p => p !== firstTeam1Server) || team1Player2;
    const otherTeam2 = team2Players.find(p => p !== firstTeam2Server) || team2Player2;
    serveOrder.value = [firstTeam1Server, firstTeam2Server, otherTeam1, otherTeam2];
  } else {
    // Team 2 serves first
    firstTeam2Server = firstServer;
    firstTeam1Server = initialReceiver || team1Player1;
    const otherTeam2 = team2Players.find(p => p !== firstTeam2Server) || team2Player2;
    const otherTeam1 = team1Players.find(p => p !== firstTeam1Server) || team1Player2;
    serveOrder.value = [firstTeam2Server, firstTeam1Server, otherTeam2, otherTeam1];
  }
}
initializeServeOrder();

const matchWinner = ref<string | null>(null);

// Edit mode state
const editMode = ref(false);
const editingPointIndex = ref<number | null>(null);

// Ref for point trail box to enable auto-scroll
const pointTrailBox = ref<HTMLElement | null>(null);

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
const tiebreakFirstServer = ref<string | null>(null);

// Point history for the trail
interface PointRecord {
  setNumber: number;
  gameNumber: number;
  pointNumber: number;
  pointWinner: string;
  firstServe: string;
  secondServe: string;
  returnStats: string;
  pointEnd: string;
  player: string;
  serveVolley: string;
  // Raw data for stat calculations
  server: string;
  pointWinnerIdx: number;
  firstServeLocationIdx: number;
  firstServeResultIdx: number;
  secondServeLocationIdx: number | null;
  secondServeResultIdx: number | null;
  returnSideIdx: number | null;
  returnTypeIdx: number | null;
  returnResultIdx: number | null;
  pointEndSideIdx: number | null;
  pointEndTypeIdx: number | null;
  pointEndResultIdx: number | null;
  playerIdx: number | null;
}
const pointHistory = ref<PointRecord[]>([]);
const currentPointInGame = ref(1);
const currentGameInSet = ref(1);

// Radar activation state
const radarActivated = ref(false);

// Summary Statistics computed properties
// Helper to get stats for a specific team
const getTeamStats = (teamPlayers: string[], teamName: string) => {
  const points = pointHistory.value;
  
  // Points where a team member was serving
  const servePoints = points.filter(p => teamPlayers.includes(p.server));
  
  // First serve stats
  const firstServeAttempts = servePoints.length;
  const firstServesIn = servePoints.filter(p => p.firstServeResultIdx !== 3).length; // Not fault
  const firstServePercentage = firstServeAttempts > 0 
    ? Math.round((firstServesIn / firstServeAttempts) * 100) 
    : 0;
  
  // First serve points won (won point when 1st serve was in)
  const firstServePointsPlayed = servePoints.filter(p => p.firstServeResultIdx !== 3);
  const firstServePointsWon = firstServePointsPlayed.filter(p => 
    p.pointWinner === teamName
  ).length;
  const firstServePointsWonPercentage = firstServePointsPlayed.length > 0
    ? Math.round((firstServePointsWon / firstServePointsPlayed.length) * 100)
    : 0;
  
  // Aces (1st serve result = 1 or 2nd serve result = 1)
  const aces = servePoints.filter(p => 
    p.firstServeResultIdx === 1 || p.secondServeResultIdx === 1
  ).length;
  
  // Double faults (2nd serve result = 3)
  const doubleFaults = servePoints.filter(p => p.secondServeResultIdx === 3).length;
  
  // All points won by this team
  const allPoints = points.filter(p => p.pointWinner === teamName);
  
  // Forehand winners (point end side = 0, result = 0, and this team won)
  const forehandWinners = allPoints.filter(p => 
    p.pointEndSideIdx === 0 && p.pointEndResultIdx === 0
  ).length;
  
  // Backhand winners (point end side = 1, result = 0, and this team won)
  const backhandWinners = allPoints.filter(p => 
    p.pointEndSideIdx === 1 && p.pointEndResultIdx === 0
  ).length;
  
  // Net points: no longer tracked since Strategy was replaced with Player
  const netPoints: PointRecord[] = [];
  const netPointsWon = 0;
  
  // Unforced errors (point end result = 1, and this team lost the point)
  const unforcedErrors = points.filter(p => 
    p.pointEndResultIdx === 1 && p.pointWinner !== teamName
  ).length;
  
  return {
    firstServePercentage,
    firstServePointsWonPercentage,
    aces,
    doubleFaults,
    forehandWinners,
    backhandWinners,
    netPointsWon,
    netPointsTotal: netPoints.length,
    unforcedErrors
  };
};

// Computed stats for each team
const topTeamStats = computed(() => getTeamStats(team1Players, topTeam));
const bottomTeamStats = computed(() => getTeamStats(team2Players, bottomTeam));

// Stat categories and their options
const statCategories = [
  'firstServeLocation',
  'firstServeResult',
  'secondServeLocation', 
  'secondServeResult',
  'returnSide',
  'returnType',
  'returnResult',
  'pointWinner',
  'pointEndSide',
  'pointEndType',
  'pointEndResult',
  'player',
  'serveVolley'
] as const;

type StatCategory = typeof statCategories[number];

// Options for each category
const serveLocationOptions = ['Wide', 'Body', 'Center', 'In Net'];
const serveResultOptions = ['In Play', 'Ace', 'Winner', 'Fault'];
const returnSideOptions = ['Forehand', 'Backhand'];
const returnTypeOptions = ['Ground', 'Pass', 'Approach', 'Lob', 'Drop'];
const returnResultOptions = ['In Play', 'Winner', 'Unforced', 'Forced'];
const pointWinnerOptions = [topTeam, bottomTeam];
const pointEndSideOptions = ['Forehand', 'Backhand'];
const pointEndTypeOptions = ['Ground', 'Pass', 'Approach', 'Lob', 'Drop', 'Overhead', 'Volley'];
const pointEndResultOptions = ['Winner', 'Unforced', 'Forced'];
const playerOptions = [team1Player1, team1Player2, team2Player1, team2Player2];
const serveVolleyOptions = ['', 'Serve & Volley'];

// Confirmation popup state
const showConfirmation = ref(false);

// Error popup state for point winner not selected
const showPointWinnerError = ref(false);

// 2nd serve is disabled if 1st serve result is "In Play" (index 0), "Ace" (1), or "Winner" (2)
// Only "Fault" (index 3) allows a 2nd serve
const secondServeDisabled = computed(() => {
  const firstResult = selections.value.firstServeResult;
  return firstResult !== null && firstResult !== 3; // Not fault = no 2nd serve
});

// Return is disabled if:
// - Serve result is "Ace" (1) or "Winner" (2) - server wins point
// - 2nd serve result is "Fault" (3) - double fault, returner wins point
const returnDisabled = computed(() => {
  if (secondServeDisabled.value) {
    // No 2nd serve, check 1st serve result
    const firstResult = selections.value.firstServeResult;
    return firstResult === 1 || firstResult === 2; // Ace or Winner
  } else {
    // 2nd serve happened, check 2nd serve result
    const secondResult = selections.value.secondServeResult;
    // Ace, Winner, or Fault (double fault) all skip return
    return secondResult === 1 || secondResult === 2 || secondResult === 3;
  }
});

// Get server team index for point winner (0 = top team, 1 = bottom team)
const serverTeamIndex = computed(() => {
  return team1Players.includes(server.value) ? 0 : 1;
});

// Get returner team index (opposite of server team)
const returnerTeamIndex = computed(() => {
  return team1Players.includes(server.value) ? 1 : 0;
});

// Point End is disabled if return result is "Winner" (1), "Unforced" (2), or "Forced" (3)
// Also disabled if return itself is disabled (ace/winner/double fault)
const pointEndDisabled = computed(() => {
  if (returnDisabled.value) {
    return true;
  }
  const returnResult = selections.value.returnResult;
  // Winner (1), Unforced (2), or Forced (3) all end the point on return
  return returnResult === 1 || returnResult === 2 || returnResult === 3;
});

// Serve & Volley is disabled when:
// - Double fault (2nd serve result is Fault)
// - Serve is Ace or Winner (1st or 2nd serve)
const serveVolleyDisabled = computed(() => {
  const firstResult = selections.value.firstServeResult;
  const secondResult = selections.value.secondServeResult;
  
  // If 1st serve was Ace (1) or Winner (2), disable
  if (firstResult === 1 || firstResult === 2) {
    return true;
  }
  
  // If 2nd serve happened (1st was fault)
  if (!secondServeDisabled.value) {
    // Double fault (Fault = 3) or Ace (1) or Winner (2)
    if (secondResult === 1 || secondResult === 2 || secondResult === 3) {
      return true;
    }
  }
  
  return false;
});

// Get the name of the point winner for confirmation popup
const confirmationWinnerName = computed(() => {
  const winnerIndex = selections.value.pointWinner ?? 0;
  return pointWinnerOptions[winnerIndex];
});

// Current active category (for keyboard navigation)
const activeCategory = ref<StatCategory>('firstServeLocation');

// Selection state for each category (null = not yet visited/selected)
const selections = ref<Record<StatCategory, number | null>>({
  firstServeLocation: 0,  // Default to Wide
  firstServeResult: null,
  secondServeLocation: null,
  secondServeResult: null,
  returnSide: null,
  returnType: null,
  returnResult: null,
  pointWinner: null,
  pointEndSide: null,
  pointEndType: null,
  pointEndResult: null,
  player: null,
  serveVolley: null
});

// Get options array for a category
function getOptionsForCategory(category: StatCategory): string[] {
  switch (category) {
    case 'firstServeLocation':
    case 'secondServeLocation':
      return serveLocationOptions;
    case 'firstServeResult':
    case 'secondServeResult':
      return serveResultOptions;
    case 'returnSide':
      return returnSideOptions;
    case 'returnType':
      return returnTypeOptions;
    case 'returnResult':
      return returnResultOptions;
    case 'pointWinner':
      return pointWinnerOptions;
    case 'pointEndSide':
      return pointEndSideOptions;
    case 'pointEndType':
      return pointEndTypeOptions;
    case 'pointEndResult':
      return pointEndResultOptions;
    case 'player':
      return playerOptions;
    case 'serveVolley':
      return serveVolleyOptions;
  }
}

// Helper to get the score for a set column
// Column 0 = first set, column 1 = second set, etc.
// Shows completed set score OR live games if it's the current set
function getSetScore(player: 'top' | 'bottom', colIndex: number): string | number {
  const sets = player === 'top' ? topSets.value : bottomSets.value;
  const games = player === 'top' ? topGames.value : bottomGames.value;
  
  // If this column has a completed set, show it
  if (colIndex < sets.length) {
    return sets[colIndex] ?? '';
  }
  // If match is over, don't show live games for unplayed sets
  if (matchWinner.value) {
    return '';
  }
  // If this is the current set column (immediately after completed sets), show live games
  if (colIndex === sets.length) {
    return games;
  }
  // Future sets - empty
  return '';
}

// Auto-select "Serve & Volley" if return type is Pass (1) or Lob (3)
function autoSelectServeVolleyIfNeeded() {
  const returnType = selections.value.returnType;
  if (returnType === 1 || returnType === 3) {
    // Pass or Lob - auto-select Serve & Volley (index 1)
    selections.value.serveVolley = 1;
  }
}

function handleKey(e: KeyboardEvent) {
  // Don't allow input after match is over
  if (matchWinner.value) {
    return;
  }
  
  // Handle error popup separately
  if (showPointWinnerError.value) {
    if (e.key === 'Enter' || e.key === 'Escape') {
      dismissPointWinnerError();
    }
    // Block all other keys when error popup is showing
    return;
  }
  
  // Handle confirmation popup separately
  if (showConfirmation.value) {
    if (e.key === 'Enter') {
      confirmPoint();
    } else if (e.key === 'Escape') {
      cancelConfirmation();
    }
    // Block all other keys when confirmation is showing
    return;
  }
  
  const currentOptions = getOptionsForCategory(activeCategory.value);
  const currentIndex = statCategories.indexOf(activeCategory.value);
  const currentSelection = selections.value[activeCategory.value];
  
  if (e.key === 'ArrowDown') {
    e.preventDefault();
    // If null, start at top (0)
    // If at bottom, clear selection (null)
    // Otherwise, move down
    if (currentSelection === null) {
      selections.value[activeCategory.value] = 0;
    } else if (currentSelection === currentOptions.length - 1) {
      // At bottom option, clear selection
      selections.value[activeCategory.value] = null;
    } else {
      selections.value[activeCategory.value] = currentSelection + 1;
    }
  } else if (e.key === 'ArrowUp') {
    e.preventDefault();
    // If null, go to bottom option
    // If at top (0), clear selection (null)
    // Otherwise, move up
    if (currentSelection === null) {
      selections.value[activeCategory.value] = currentOptions.length - 1;
    } else if (currentSelection === 0) {
      // At top option, clear selection
      selections.value[activeCategory.value] = null;
    } else {
      selections.value[activeCategory.value] = currentSelection - 1;
    }
  } else if (e.key === 'ArrowRight') {
    e.preventDefault();
    // Check if leaving a serve location box with "In Net" selected
    // If so, auto-set result to "Fault" and skip to next box after result
    if (activeCategory.value === 'firstServeLocation' && selections.value.firstServeLocation === 3) {
      // "In Net" selected - auto-set result to "Fault" (index 3) and skip to secondServeLocation
      selections.value.firstServeResult = 3;
      activeCategory.value = 'secondServeLocation';
      if (selections.value.secondServeLocation === null) {
        selections.value.secondServeLocation = 0;
      }
    } else if (activeCategory.value === 'firstServeResult' && secondServeDisabled.value) {
      // 1st serve was in play/ace/winner - skip 2nd serve
      // If ace or winner, also skip return and auto-select server as point winner
      const firstResult = selections.value.firstServeResult;
      if (firstResult === 1 || firstResult === 2) {
        // Ace or Winner - skip to pointWinner, auto-select server
        selections.value.pointWinner = serverTeamIndex.value;
        activeCategory.value = 'pointWinner';
      } else {
        // In Play - go to returnSide
        activeCategory.value = 'returnSide';
        if (selections.value.returnSide === null) {
          selections.value.returnSide = 0;
        }
      }
    } else if (activeCategory.value === 'secondServeLocation' && selections.value.secondServeLocation === 3) {
      // "In Net" selected - auto-set result to "Fault" (double fault)
      // Skip return, auto-select returner as point winner
      selections.value.secondServeResult = 3;
      selections.value.pointWinner = returnerTeamIndex.value;
      activeCategory.value = 'pointWinner';
    } else if (activeCategory.value === 'secondServeResult') {
      // Check 2nd serve result
      const secondResult = selections.value.secondServeResult;
      if (secondResult === 1 || secondResult === 2) {
        // Ace or Winner - skip return, auto-select server
        selections.value.pointWinner = serverTeamIndex.value;
        activeCategory.value = 'pointWinner';
      } else if (secondResult === 3) {
        // Fault (double fault) - skip return, auto-select returner
        selections.value.pointWinner = returnerTeamIndex.value;
        activeCategory.value = 'pointWinner';
      } else {
        // In Play - normal navigation to returnSide
        activeCategory.value = 'returnSide';
        if (selections.value.returnSide === null) {
          selections.value.returnSide = 0;
        }
      }
    } else if (activeCategory.value === 'returnType') {
      // Auto-select Serve & Volley if return type is Pass or Lob when leaving this box
      autoSelectServeVolleyIfNeeded();
      // Normal navigation to returnResult
      activeCategory.value = 'returnResult';
      if (selections.value.returnResult === null) {
        selections.value.returnResult = 0;
      }
    } else if (activeCategory.value === 'returnResult') {
      // Check if return result is Winner - skip point end, auto-select returner
      const returnResult = selections.value.returnResult;
      if (returnResult === 1) {
        // Winner - skip point end, auto-select returner
        selections.value.pointWinner = returnerTeamIndex.value;
        activeCategory.value = 'pointWinner';
      } else {
        // In Play, Unforced, or Forced - normal navigation to pointWinner
        const nextIndex = (currentIndex + 1) % statCategories.length;
        const nextCategory = statCategories[nextIndex]!;
        activeCategory.value = nextCategory;
        if (selections.value[nextCategory] === null) {
          selections.value[nextCategory] = 0;
        }
      }
    } else if (activeCategory.value === 'player') {
      // If moving from player to serveVolley but serveVolley is disabled, wrap to firstServeLocation
      if (serveVolleyDisabled.value) {
        activeCategory.value = 'firstServeLocation';
        selections.value.firstServeLocation = 0;
      } else {
        activeCategory.value = 'serveVolley';
        if (selections.value.serveVolley === null) {
          selections.value.serveVolley = 0;
        }
      }
    } else if (activeCategory.value === 'pointEndResult') {
      // Navigate to player box
      activeCategory.value = 'player';
      if (selections.value.player === null) {
        selections.value.player = 0;
      }
    } else {
      // Normal navigation - move to next category
      const nextIndex = (currentIndex + 1) % statCategories.length;
      const nextCategory = statCategories[nextIndex]!;
      activeCategory.value = nextCategory;
      // If entering a box for the first time, set selection to 0 (first option)
      if (selections.value[nextCategory] === null) {
        selections.value[nextCategory] = 0;
      }
    }
  } else if (e.key === 'ArrowLeft') {
    e.preventDefault();
    // Auto-select Serve & Volley if leaving return area with Pass or Lob selected
    if (activeCategory.value === 'returnSide' || activeCategory.value === 'returnResult') {
      autoSelectServeVolleyIfNeeded();
    }
    // If at pointWinner and point end is disabled due to return winner, skip back to returnResult
    if (activeCategory.value === 'pointWinner' && pointEndDisabled.value && !returnDisabled.value) {
      activeCategory.value = 'returnResult';
    } else if (activeCategory.value === 'pointWinner' && returnDisabled.value) {
      // If return is disabled, skip back to serve result
      if (secondServeDisabled.value) {
        activeCategory.value = 'firstServeResult';
      } else {
        activeCategory.value = 'secondServeResult';
      }
    } else if (activeCategory.value === 'returnSide' && secondServeDisabled.value) {
      // If at returnSide and 2nd serve is disabled, skip back to firstServeResult
      activeCategory.value = 'firstServeResult';
    } else if (activeCategory.value === 'firstServeLocation' && serveVolleyDisabled.value) {
      // If at first box and serveVolley is disabled, skip back to player
      activeCategory.value = 'player';
    } else {
      // Normal navigation - move to previous category
      const prevIndex = (currentIndex - 1 + statCategories.length) % statCategories.length;
      const prevCategory = statCategories[prevIndex]!;
      activeCategory.value = prevCategory;
      // If entering a box for the first time, set selection to 0 (first option)
      if (selections.value[prevCategory] === null) {
        selections.value[prevCategory] = 0;
      }
    }
  } else if (e.key === 'a' || e.key === 'A') {
    // Activate radar
    radarActivated.value = true;
  } else if (e.key === 'c' || e.key === 'C') {
    // Clear/deactivate radar
    radarActivated.value = false;
  } else if (e.key === 'Enter') {
    // Check if at serve result with Ace or Winner - auto-select server as point winner
    const isFirstServeAceOrWinner = activeCategory.value === 'firstServeResult' && 
      (selections.value.firstServeResult === 1 || selections.value.firstServeResult === 2);
    const isSecondServeAceOrWinner = activeCategory.value === 'secondServeResult' && 
      (selections.value.secondServeResult === 1 || selections.value.secondServeResult === 2);
    
    // Check if at 2nd serve location with "In Net" selected - double fault, returner wins
    const isSecondServeInNet = activeCategory.value === 'secondServeLocation' && 
      selections.value.secondServeLocation === 3;
    
    // Check if at 2nd serve result with "Fault" selected - double fault, returner wins
    const isSecondServeFault = activeCategory.value === 'secondServeResult' && 
      selections.value.secondServeResult === 3;
    
    // Check if at return result with "Winner" selected - returner wins
    const isReturnWinner = activeCategory.value === 'returnResult' && 
      selections.value.returnResult === 1;
    
    // Check if at return result with "Unforced" (2) or "Forced" (3) selected - server wins
    const isReturnError = activeCategory.value === 'returnResult' && 
      (selections.value.returnResult === 2 || selections.value.returnResult === 3);
    
    if (isFirstServeAceOrWinner || isSecondServeAceOrWinner || isReturnError) {
      // Auto-select server as point winner
      selections.value.pointWinner = serverTeamIndex.value;
    } else if (isSecondServeInNet || isSecondServeFault || isReturnWinner) {
      // Returner wins - double fault or return winner
      if (isSecondServeInNet) {
        selections.value.secondServeResult = 3; // Set result to Fault
      }
      selections.value.pointWinner = returnerTeamIndex.value;
    }
    
    // Validate that point winner is selected
    if (selections.value.pointWinner === null) {
      showPointWinnerError.value = true;
      return;
    }
    
    // Show confirmation popup
    showConfirmation.value = true;
  }
}

function dismissPointWinnerError() {
  showPointWinnerError.value = false;
}

function confirmPoint() {
  // Hide the confirmation popup
  showConfirmation.value = false;
  
  if (editMode.value) {
    // Update existing point
    updateEditedPoint();
  } else {
    // Process new point
    processPoint();
  }
}

function cancelConfirmation() {
  // Hide the confirmation popup, keep selections as-is for editing
  showConfirmation.value = false;
}

// Update an existing point in edit mode
function updateEditedPoint() {
  if (editingPointIndex.value === null) return;
  
  const getSel = (cat: StatCategory) => selections.value[cat] ?? 0;
  
  // Format first serve: "Location, Result"
  const firstServe = `${serveLocationOptions[getSel('firstServeLocation')]}, ${serveResultOptions[getSel('firstServeResult')]}`;
  
  // Format second serve: "Location, Result" or empty if not used
  const secondServeUsed = getSel('firstServeResult') === 3; // Fault
  const secondServe = secondServeUsed 
    ? `${serveLocationOptions[getSel('secondServeLocation')]}, ${serveResultOptions[getSel('secondServeResult')]}`
    : '';
  
  // Format return: "Side, Type, Result" or empty if skipped
  const returnSkipped = returnDisabled.value;
  const returnStats = returnSkipped
    ? ''
    : `${returnSideOptions[getSel('returnSide')]}, ${returnTypeOptions[getSel('returnType')]}, ${returnResultOptions[getSel('returnResult')]}`;
  
  // Format point end: "Side, Type, Result" or empty if skipped
  const pointEndSkipped = pointEndDisabled.value;
  const pointEnd = pointEndSkipped
    ? ''
    : `${pointEndSideOptions[getSel('pointEndSide')]}, ${pointEndTypeOptions[getSel('pointEndType')]}, ${pointEndResultOptions[getSel('pointEndResult')]}`;
  
  // Get player (empty if point end is disabled/skipped) and serve & volley
  const player = pointEndSkipped ? '' : (playerOptions[getSel('player')] ?? '');
  const serveVolley = serveVolleyOptions[getSel('serveVolley')] ?? '';
  
  // Keep the original point metadata (set, game, point numbers, server)
  const originalPoint = pointHistory.value[editingPointIndex.value];
  if (!originalPoint) return;
  
  // Update the point record
  pointHistory.value[editingPointIndex.value] = {
    setNumber: originalPoint.setNumber,
    gameNumber: originalPoint.gameNumber,
    pointNumber: originalPoint.pointNumber,
    server: originalPoint.server,
    pointWinner: pointWinnerOptions[getSel('pointWinner')] ?? '',
    firstServe,
    secondServe,
    returnStats,
    pointEnd,
    player,
    serveVolley,
    // Raw data for statistics
    pointWinnerIdx: getSel('pointWinner'),
    firstServeLocationIdx: getSel('firstServeLocation'),
    firstServeResultIdx: getSel('firstServeResult'),
    secondServeLocationIdx: secondServeUsed ? getSel('secondServeLocation') : null,
    secondServeResultIdx: secondServeUsed ? getSel('secondServeResult') : null,
    returnSideIdx: returnSkipped ? null : getSel('returnSide'),
    returnTypeIdx: returnSkipped ? null : getSel('returnType'),
    returnResultIdx: returnSkipped ? null : getSel('returnResult'),
    pointEndSideIdx: pointEndSkipped ? null : getSel('pointEndSide'),
    pointEndTypeIdx: pointEndSkipped ? null : getSel('pointEndType'),
    pointEndResultIdx: pointEndSkipped ? null : getSel('pointEndResult'),
    playerIdx: pointEndSkipped ? null : getSel('player')
  };
  
  console.log('Point updated:', pointHistory.value[editingPointIndex.value]);
  
  // Reset radar activation
  radarActivated.value = false;
  
  // Exit edit mode
  exitEditMode();
}

function processPoint() {
  // Helper to get selection value (default to 0 if null/unvisited)
  const getSel = (cat: StatCategory) => selections.value[cat] ?? 0;
  
  // Format first serve: "Location, Result"
  const firstServe = `${serveLocationOptions[getSel('firstServeLocation')]}, ${serveResultOptions[getSel('firstServeResult')]}`;
  
  // Format second serve: "Location, Result" or empty if not used
  const secondServeUsed = getSel('firstServeResult') === 3; // Fault
  const secondServe = secondServeUsed 
    ? `${serveLocationOptions[getSel('secondServeLocation')]}, ${serveResultOptions[getSel('secondServeResult')]}`
    : '';
  
  // Format return: "Side, Type, Result" or empty if skipped
  const returnSkipped = returnDisabled.value;
  const returnStats = returnSkipped
    ? ''
    : `${returnSideOptions[getSel('returnSide')]}, ${returnTypeOptions[getSel('returnType')]}, ${returnResultOptions[getSel('returnResult')]}`;
  
  // Format point end: "Side, Type, Result" or empty if skipped
  const pointEndSkipped = pointEndDisabled.value;
  const pointEnd = pointEndSkipped
    ? ''
    : `${pointEndSideOptions[getSel('pointEndSide')]}, ${pointEndTypeOptions[getSel('pointEndType')]}, ${pointEndResultOptions[getSel('pointEndResult')]}`;
  
  // Get player (empty if point end is disabled/skipped) and serve & volley
  const player = pointEndSkipped ? '' : (playerOptions[getSel('player')] ?? '');
  const serveVolley = serveVolleyOptions[getSel('serveVolley')] ?? '';
  
  // Create point record with raw index data for stats calculations
  const pointRecord: PointRecord = {
    setNumber: currentSet.value,
    gameNumber: currentGameInSet.value,
    pointNumber: currentPointInGame.value,
    pointWinner: pointWinnerOptions[getSel('pointWinner')] ?? '',
    firstServe,
    secondServe,
    returnStats,
    pointEnd,
    player,
    serveVolley,
    // Raw data for statistics
    server: server.value,
    firstServeLocationIdx: getSel('firstServeLocation'),
    firstServeResultIdx: getSel('firstServeResult'),
    secondServeLocationIdx: secondServeUsed ? getSel('secondServeLocation') : null,
    secondServeResultIdx: secondServeUsed ? getSel('secondServeResult') : null,
    returnSideIdx: returnSkipped ? null : getSel('returnSide'),
    returnTypeIdx: returnSkipped ? null : getSel('returnType'),
    returnResultIdx: returnSkipped ? null : getSel('returnResult'),
    pointEndSideIdx: pointEndSkipped ? null : getSel('pointEndSide'),
    pointEndTypeIdx: pointEndSkipped ? null : getSel('pointEndType'),
    pointEndResultIdx: pointEndSkipped ? null : getSel('pointEndResult'),
    playerIdx: pointEndSkipped ? null : getSel('player'),
    pointWinnerIdx: getSel('pointWinner')
  };
  
  // Add to history
  pointHistory.value.push(pointRecord);
  
  // Scroll point trail to bottom to show latest point
  nextTick(() => {
    if (pointTrailBox.value) {
      pointTrailBox.value.scrollTop = pointTrailBox.value.scrollHeight;
    }
  });
  
  // Increment point counter
  currentPointInGame.value++;
  
  console.log('Point recorded:', pointRecord);
  
  // Update score based on point winner (default to top player if not selected)
  const pointWinnerSelection = getSel('pointWinner');
  if (inTiebreak.value) {
    // Tiebreak scoring
    if (pointWinnerSelection === 0) {
      topTiebreakPoints.value++;
    } else {
      bottomTiebreakPoints.value++;
    }
    // Tiebreak serve switching: first serve is 1 point, then alternate every 2 points
    const totalPoints = topTiebreakPoints.value + bottomTiebreakPoints.value;
    if (totalPoints === 1 || (totalPoints > 1 && (totalPoints - 1) % 2 === 0)) {
      switchServer();
    }
    checkTiebreakWin();
  } else {
    // Regular game scoring
    if (pointWinnerSelection === 0) {
      updateScore(topPoint, bottomPoint, topGames, bottomGames);
    } else {
      updateScore(bottomPoint, topPoint, bottomGames, topGames);
    }
  }
  
  // Reset radar activation for next point
  radarActivated.value = false;
  
  // Reset selections to defaults for next point (except point winner stays at 0)
  resetSelectionsForNextPoint();
}

function resetSelectionsForNextPoint() {
  // Reset all selections to null (unvisited) except first box
  selections.value.firstServeLocation = 0;  // First box starts selected
  selections.value.firstServeResult = null;
  selections.value.secondServeLocation = null;
  selections.value.secondServeResult = null;
  selections.value.returnSide = null;
  selections.value.returnType = null;
  selections.value.returnResult = null;
  selections.value.pointWinner = null;
  selections.value.pointEndSide = null;
  selections.value.pointEndType = null;
  selections.value.pointEndResult = null;
  selections.value.player = null;
  selections.value.serveVolley = null;
  // Reset active category to first one
  activeCategory.value = 'firstServeLocation';
}

// Enter edit mode for a specific point
function enterEditMode(pointIndex: number) {
  const point = pointHistory.value[pointIndex];
  if (!point) return;
  
  editMode.value = true;
  editingPointIndex.value = pointIndex;
  
  // Load the point's selections into the current selections
  selections.value.firstServeLocation = point.firstServeLocationIdx;
  selections.value.firstServeResult = point.firstServeResultIdx;
  selections.value.secondServeLocation = point.secondServeLocationIdx;
  selections.value.secondServeResult = point.secondServeResultIdx;
  selections.value.returnSide = point.returnSideIdx;
  selections.value.returnType = point.returnTypeIdx;
  selections.value.returnResult = point.returnResultIdx;
  selections.value.pointWinner = point.pointWinnerIdx;
  selections.value.pointEndSide = point.pointEndSideIdx;
  selections.value.pointEndType = point.pointEndTypeIdx;
  selections.value.pointEndResult = point.pointEndResultIdx;
  selections.value.player = point.playerIdx;
  selections.value.serveVolley = serveVolleyOptions.indexOf(point.serveVolley);
  if (selections.value.serveVolley === -1) selections.value.serveVolley = 0;
  
  // Set active category to first box
  activeCategory.value = 'firstServeLocation';
}

// Exit edit mode without saving
function exitEditMode() {
  editMode.value = false;
  editingPointIndex.value = null;
  resetSelectionsForNextPoint();
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

function switchServer() {
  // Move to next in serve rotation (cycles through all 4 players)
  currentServeOrderIndex.value = (currentServeOrderIndex.value + 1) % 4;
  server.value = serveOrder.value[currentServeOrderIndex.value] || initialServer;
}

function winGame(myGames: { value: number }, _oppGames: { value: number }) {
  myGames.value++;
  currentPointInGame.value = 1; // Reset point counter for new game
  currentGameInSet.value++; // Increment game counter
  switchServer(); // Server alternates after each game
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
    tiebreakFirstServer.value = server.value; // Remember who serves first in tiebreak
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

function winSet(_winner: 'top' | 'bottom') {
  // Store completed set scores
  topSets.value.push(topGames.value);
  bottomSets.value.push(bottomGames.value);

  // Check if match is won (best of 3 = first to 2 sets)
  const topSetsWon = topSets.value.filter((s, i) => s > (bottomSets.value[i] ?? 0)).length;
  const bottomSetsWon = bottomSets.value.filter((s, i) => s > (topSets.value[i] ?? 0)).length;

  if (topSetsWon >= 2) {
    matchWinner.value = topTeam;
    return;
  }
  if (bottomSetsWon >= 2) {
    matchWinner.value = bottomTeam;
    return;
  }

  // Reset for next set
  topGames.value = 0;
  bottomGames.value = 0;
  topPoint.value = 0;
  bottomPoint.value = 0;
  
  // After a tiebreak, continue serve rotation from where it left off
  // The next server in order should serve first in the new set
  if (inTiebreak.value) {
    // Already on correct server from tiebreak rotation
  }
  
  inTiebreak.value = false;
  topTiebreakPoints.value = 0;
  bottomTiebreakPoints.value = 0;
  tiebreakFirstServer.value = null;
  currentSet.value++;
  currentGameInSet.value = 1; // Reset game counter for new set
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
  position: relative;
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
}
.score-row {
  display: flex;
}
.player-cell {
  min-width: 400px;
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

/* Top Row (Score Box + Radar Box) */
.top-row {
  display: flex;
  align-items: stretch;
  justify-content: space-between;
  margin-bottom: 24px;
}

/* Radar Box */
.radar-box {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 160px;
  padding: 0 24px;
  border: 2px solid #111;
  background-color: #ffd700;
  font-weight: 600;
  font-size: 1rem;
  cursor: default;
}
.radar-box.radar-active {
  background-color: #4caf50;
  color: #fff;
}

/* Point Trail */
.point-trail-box {
  border: 2px solid #111;
  max-height: 135px;
  padding: 0;
  margin-bottom: 24px;
  overflow: auto;
}
.point-trail-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
  table-layout: fixed;
}
.point-trail-table th,
.point-trail-table td {
  border: 1px solid #ccc;
  padding: 8px 6px;
  text-align: center;
  vertical-align: middle;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.point-trail-table th {
  background-color: #f5f5f5;
  font-weight: 600;
  position: sticky;
  top: 0;
  z-index: 1;
}
.point-trail-table tbody tr:nth-child(even) {
  background-color: #fafafa;
}
.point-trail-table tbody tr.empty-row {
  background-color: transparent;
}
.point-trail-row {
  cursor: pointer;
  transition: background-color 0.15s;
}
.point-trail-row:hover {
  background-color: #e3f2fd !important;
}
.point-trail-row.editing-row {
  background-color: #fff59d !important;
}
.point-trail-label {
  color: #aaa;
  font-size: 1rem;
}

/* Edit Mode Banner */
.edit-mode-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: #fff59d;
  border: 2px solid #f9a825;
  padding: 8px 16px;
  margin-bottom: 16px;
  font-weight: 600;
}
.cancel-edit-btn {
  background: #888;
  color: #fff;
  border: none;
  padding: 6px 16px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  border-radius: 4px;
}
.cancel-edit-btn:hover {
  background: #666;
}

/* Stat Rows */
.stat-row {
  display: flex;
  gap: 0;
  margin-bottom: 24px;
  align-items: stretch;
  width: 100%;
}
.stat-group {
  display: flex;
  flex-direction: column;
}
.stat-group:last-child {
  flex: 1;
}
.stat-label {
  font-size: 0.95rem;
  font-weight: 600;
  margin-bottom: 4px;
  padding-left: 4px;
  min-height: 20px;
}
.stat-label-center {
  font-size: 0.95rem;
  font-weight: 600;
  margin-bottom: 4px;
  min-height: 20px;
  text-align: center;
}
.stat-box {
  display: flex;
  border: 2px solid #111;
  flex: 1;
}
.stat-box-single {
  min-width: 100px;
}
.stat-box-strategy {
  min-width: 160px;
}
.stat-box-sv {
  min-width: 120px;
}
.stat-group-strategy {
  flex: 2;
}
.stat-group-expand {
  flex: 1;
}
.summary-row-container {
  margin-top: 16px;
}
.stat-box-summary {
  min-width: 700px;
  display: flex;
  flex-direction: column;
  padding: 16px 24px;
}
.summary-stats-group .stat-label-center {
  font-weight: 600;
  color: #111;
}
.summary-stats-container {
  display: flex;
  flex-direction: column;
  gap: 6px;
  width: 100%;
}
.summary-header-row {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}
.summary-player-header {
  flex: 2;
  text-align: center;
  font-size: 1rem;
  font-weight: 500;
  color: #666;
}
.summary-center-spacer {
  flex: 1.5;
}
.summary-subheader-row {
  display: flex;
  align-items: center;
  margin-bottom: 8px;
}
.summary-col-label {
  flex: 1;
  text-align: center;
  font-size: 0.9rem;
  font-weight: 500;
  color: #333;
}
.summary-data-row {
  display: flex;
  align-items: center;
}
.summary-stat-label {
  flex: 1.5;
  text-align: center;
  font-size: 0.9rem;
  color: #888;
}
.summary-val {
  flex: 1;
  text-align: center;
  font-size: 0.9rem;
  color: #111;
}
.summary-header {
  display: flex;
  border-bottom: 1px solid #ccc;
  padding-bottom: 4px;
  margin-bottom: 2px;
}
.summary-header .summary-player-name {
  font-weight: 600;
  font-size: 0.85rem;
  color: #111;
}
.summary-row {
  display: flex;
  font-size: 0.85rem;
}
.summary-stat-name {
  flex: 2;
  text-align: left;
  color: #666;
  font-size: 0.8rem;
  white-space: nowrap;
}
.summary-player-name,
.summary-value {
  flex: 1;
  text-align: center;
  color: #111;
  font-weight: 500;
}
.stat-col {
  padding: 0;
  border-right: 2px solid #111;
  display: flex;
  flex-direction: column;
  gap: 0;
  font-size: 0.95rem;
  text-align: center;
  flex: 1;
}
.stat-col:last-child {
  border-right: none;
}
.stat-box-row {
  display: flex;
  gap: 0;
  flex: 1;
  align-items: stretch;
}
.stat-box-row .stat-box {
  flex: none;
}
.point-winner-box {
  flex-direction: column;
  padding: 0;
}
.point-winner-option {
  padding: 4px 12px;
  font-size: 0.95rem;
  text-align: center;
  cursor: pointer;
}
.stat-option {
  cursor: pointer;
  padding: 4px 12px;
  width: 100%;
  box-sizing: border-box;
}
.stat-option:hover,
.point-winner-option:hover {
  background-color: #e0e0e0;
}
.selectable-box {
  cursor: pointer;
}
.active-box {
  border-color: #8e267d;
  border-width: 3px;
}
.selected-row {
  background-color: #fff59d;
}
.disabled-group {
  opacity: 0.4;
}
.disabled-box {
  background-color: #e0e0e0;
  cursor: not-allowed;
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

/* Confirmation popup buttons */
.popup-buttons {
  display: flex;
  gap: 16px;
  justify-content: center;
}
.confirm-btn {
  border: 2px solid #111;
  padding: 10px 32px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
}
.yes-btn {
  background: #8e267d;
  color: #fff;
  border-color: #8e267d;
}
.yes-btn:hover {
  background: #6d1d5f;
  border-color: #6d1d5f;
}
.no-btn {
  background: #fff;
  color: #111;
}
.no-btn:hover {
  background: #f0f0f0;
}
</style>
