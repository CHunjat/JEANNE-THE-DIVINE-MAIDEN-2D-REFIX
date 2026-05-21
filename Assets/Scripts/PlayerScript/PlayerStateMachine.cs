public class PlayerStateMachine
{
    // 현재 캐릭터가 어떤 상태인지 기억하는 변수
    public PlayerState CurrentState { get; private set; }

    // 게임 시작 시 첫 번째 상태(보통 Idle)를 설정하는 함수
    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter(); // 첫 상태의 진입 로직 실행
    }

    // 상태를 바꿀 때 사용하는 함수 (예: Idle -> Move)
    public void ChangeState(PlayerState newState)
    {
        CurrentState.Exit();  // 이전 상태를 끝내고
        CurrentState = newState; // 새 상태를 저장한 뒤
        CurrentState.Enter(); // 새 상태의 진입 로직 실행
    }
}