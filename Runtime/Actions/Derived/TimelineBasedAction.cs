namespace ActionBuilder.Runtime
{
    public class TimelineBasedAction : ActionBase
    {
        //effect별, delay(시작 시간, 끝시간은 delay + duration)를 저장해둠. 
        
        
        protected override void OnValidateAction()
        {
            //effect가 추가될때마다, 여기서 새 TimelineData를 추가.
            //TimeLine Data의 허용 한도는 effect의 delay+duration이 action의 duration을 넘으면 안 됨.
            //delay+duration이 action의 duration과 1:1 비율은 허용되지만, 넘으면 안됨. aciton duration보다 작으면 좋음.
        }


        protected override void OnUpdate(float deltaTime)
        {
            //여기서 재생시킴.
        }
    }
}